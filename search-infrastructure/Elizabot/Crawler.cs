using HtmlAgilityPack;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Mono.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Elizabot
{
    public class Crawler
    {
        private Dictionary<string, Host> hosts;
        private List<string> visitedXmls;

        private CloudQueue robotQueue;
        private CloudQueue urlQueue;

        private CloudTable pagesTable;
        private CloudTable errorsTable;

        public Crawler(CloudQueue robotQueue, CloudQueue urlQueue, CloudTable pagesTable, CloudTable statsTable, CloudTable errorsTable)
        {
            hosts = new Dictionary<string, Host>();
            visitedXmls = new List<string>();
            this.robotQueue = robotQueue;
            this.urlQueue = urlQueue;
            this.pagesTable = pagesTable;
            this.errorsTable = errorsTable;
        }

        //returns a tuple of the increased index size and increased url queue size
        public Tuple<int, int> crawlSite(string url)
        {
            int updateIndex = 0;
            int updateQueue = -1;

            try
            {
                Uri uri = new Uri(url);
                Host host;

                if (hosts.TryGetValue(uri.Host, out host))
                {
                    if (host.isAllowed(uri))
                    {
                        //check if url has been visited before
                        if (!host.hasVisited(uri))
                        {
                            HtmlDocument htmlDoc;

                            HtmlWeb web = new HtmlWeb();
                            htmlDoc = web.Load(uri.AbsoluteUri);

                            if (web.StatusCode == HttpStatusCode.OK)
                            {
                                string title = "";
                                string date = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
                                string body = "";

                                host.addVisited(uri);

                                //get title
                                HtmlNode metaTitleNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='title']");
                                if (metaTitleNode != null)
                                {
                                    title = metaTitleNode.GetAttributeValue("content", "");
                                    body = title;
                                } else {
                                    HtmlNode metaOgTitleNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='og:title']");
                                    if (metaOgTitleNode != null)
                                    {
                                        title = metaOgTitleNode.GetAttributeValue("content", "");
                                        body = title;
                                    } else
                                    {
                                        HtmlNode titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
                                        if (titleNode != null)
                                        {
                                            title = HttpUtility.HtmlDecode(titleNode.InnerHtml);
                                            body = title;
                                        }
                                    }
                                }

                                //get last mod date of page
                                HtmlNode modNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='lastmod']");
                                if (modNode != null)
                                {
                                    date = modNode.GetAttributeValue("content", "");
                                }
                                else
                                {
                                    //if no last mod date, check if there is a pub date 
                                    HtmlNode pubNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='pubdate']");
                                    if (pubNode != null)
                                    {
                                        date = pubNode.GetAttributeValue("content", "");
                                    }
                                }

                                //get body of page
                                HtmlNode metaDescNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='description']");
                                if (metaDescNode != null)
                                {
                                    body = metaDescNode.GetAttributeValue("content", "");
                                }
                                else
                                {
                                    HtmlNode metaOgDescNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='og:description']");
                                    if (metaOgDescNode != null)
                                    {
                                        body = metaOgDescNode.GetAttributeValue("content", "");
                                    }
                                }

                                //HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//p[contains(@class,'zn-body__paragraph')]");
                                //if (bodyNode != null)
                                //{
                                //    if (body.Length > 200)
                                //    {
                                //        body = Operation.stripHtml(bodyNode.InnerText).Substring(0, 200) + "...";
                                //    }
                                //}

                                //Insert page with each word in the title as a row key
                                string[] keyWord = Operation.stripPunct(title.ToLower()).Split().Distinct().ToArray();
                                foreach (string key in keyWord)
                                {
                                    if (key != "")
                                    {
                                        try
                                        {
                                            //get page data and store to table
                                            PageEntity page = new PageEntity(uri, title, date, body, key);
                                            TableOperation insertOperation = TableOperation.Insert(page);
                                            pagesTable.Execute(insertOperation);
                                            updateIndex++;
                                        }
                                        catch (Exception e)
                                        {
                                            //Insert error to table
                                            ErrorEntity err = new ErrorEntity(url, e.Message, DateTime.Now.ToString());
                                            TableOperation insertOperation = TableOperation.Insert(err);
                                            errorsTable.ExecuteAsync(insertOperation);

                                            Console.Write(e.ToString());
                                        }
                                    }
                                }

                                HtmlNode[] linkNodes = new HtmlNode[0];
                                HtmlNodeCollection tempNodes = htmlDoc.DocumentNode.SelectNodes("//a");
                                if (tempNodes != null)
                                {
                                    linkNodes = tempNodes.ToArray();
                                }
                                Uri newUri;
                                foreach (HtmlNode link in linkNodes)
                                {
                                    //add url if within allowed domain
                                    try
                                    {
                                        newUri = new Uri(uri, link.GetAttributeValue("href", null));
                                        if (Operation.domains.Values.Any(newUri.Host.Contains))
                                        {
                                            if (newUri.Host.Contains(Operation.domains["BR1"]) || newUri.Host.Contains(Operation.domains["BR2"]))
                                            {
                                                if (newUri.AbsolutePath.StartsWith(Operation._BR_PATH))
                                                {
                                                    CloudQueueMessage urlMessage = new CloudQueueMessage(newUri.AbsoluteUri);
                                                    urlQueue.AddMessageAsync(urlMessage);
                                                    updateQueue++;
                                                }
                                            } else
                                            {
                                                CloudQueueMessage urlMessage = new CloudQueueMessage(newUri.AbsoluteUri);
                                                urlQueue.AddMessageAsync(urlMessage);
                                                updateQueue++;
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        //Invalid url
                                        Console.WriteLine("Invalid html url found: " + e.ToString());
                                    }
                                }

                                
                            }
                        }
                    }
                }
                else
                {
                    //if robots.txt has not been parsed for the given url site 
                    //and is within domain
                    if (Operation.domains.Values.Any(uri.Host.Contains))
                    {
                        //add to xmlqueue and add url back into urlqueue
                        CloudQueueMessage robotMessage = new CloudQueueMessage(uri.AbsoluteUri);
                        robotQueue.AddMessage(robotMessage);

                        CloudQueueMessage urlMessage = new CloudQueueMessage(uri.AbsoluteUri);
                        urlQueue.AddMessage(urlMessage);
                        updateQueue++;
                    }
                }
            }
            catch (Exception e)
            {
                //Insert error to table
                ErrorEntity err = new ErrorEntity(url, e.Message, DateTime.Now.ToString());
                try
                {
                    TableOperation insertOperation = TableOperation.Insert(err);
                    errorsTable.ExecuteAsync(insertOperation);
                }
                catch (Exception insErr)
                {
                    Console.Write(insErr.ToString());
                }
            }
            return new Tuple<int, int>(updateIndex, updateQueue);
        }

        public int parseRobots(string url)
        {
            int counter = 0;
            try
            {
                Uri uri = new Uri(url, UriKind.Absolute);
                Uri robo = new Uri(uri, "/robots.txt");
                string host = uri.Host;

                //parse robots if robots has not yet been read for this host
                //also parse xmls sitemaps
                if (!hosts.ContainsKey(host))
                {
                    string initialSiteMap = null;
                    //add only nba bleacherreport site map if host is bleacherreport
                    if (host == Operation.domains["BR1"] || host == Operation.domains["BR2"])
                    {
                        initialSiteMap = Operation._BR_SITEMAP;
                    }
                    //else if (host == Operation.domains["BBC1"] || host == Operation.domains["BBC2"])
                    //{
                    //    initialSiteMap = Operation._BBC_SITEMAP;
                    //}

                    List<string> xmls = readRobots(host, robo, initialSiteMap);
                    foreach (string xml in xmls)
                    {
                        visitedXmls.Add(xml);
                        counter += parseXml(xml, host, 0);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error with url " + url + ". " + e.ToString());
            }

            return counter;
        }

        private List<string> readRobots(string hostString, Uri uri, string initialSiteMap)
        {
            List<string> xmls = new List<string>();
            Host host = new Host(hostString);

            //add given sitemap to xml queue
            if (initialSiteMap != null)
            {
                xmls.Add(initialSiteMap);
            }

            try
            {
                // Create a request for the URL
                WebRequest request = WebRequest.Create(uri.ToString());
                // If required by the server, set the credentials.  
                request.Credentials = CredentialCache.DefaultCredentials;

                // Get the response
                using (WebResponse response = request.GetResponse())
                {
                    // Get the stream containing content returned by the server.  
                    Stream dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.  
                    using (StreamReader sr = new StreamReader(dataStream))
                    {
                        string line = "";
                        Boolean user = false;
                        while ((line = sr.ReadLine()) != null)
                        {
                            try
                            {
                                string[] parsed = line.Split(' ');

                                //check for blank lines, etc
                                if (parsed.Length == 2)
                                {
                                    string first = parsed[0];
                                    string second = parsed[1];

                                    if (string.Equals(first, "User-agent:", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (second == "*")
                                        {
                                            user = true;
                                        }
                                        else
                                        {
                                            user = false;
                                        }
                                    }
                                    else if (string.Equals(first, "Sitemap:", StringComparison.OrdinalIgnoreCase) && initialSiteMap == null)
                                    {
                                        if (!visitedXmls.Contains(second))
                                        {
                                            xmls.Add(second);
                                        }
                                    }
                                    else if (string.Equals(first, "Allow:", StringComparison.OrdinalIgnoreCase) && user)
                                    {
                                        host.addAllow(second);
                                    }
                                    else if (string.Equals(first, "Disallow:", StringComparison.OrdinalIgnoreCase) && user)
                                    {
                                        host.addDisallow(second);
                                    }
                                }

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error parsing line of robots.txt. " + e.ToString());
                            }
                        }
                    }
                    response.Close();

                    // Clean up the streams and the response.  
                    if (hostString == Operation.domains["CNN1"] || hostString == Operation.domains["CNN2"])
                    {
                        hosts.Add(Operation.domains["CNN1"], host);
                        hosts.Add(Operation.domains["CNN2"], host);
                    }
                    else if (hostString == Operation.domains["BR1"] || hostString == Operation.domains["BR2"])
                    {
                        hosts.Add(Operation.domains["BR1"], host);
                        hosts.Add(Operation.domains["BR2"], host);
                    }
                    else if (hostString == Operation.domains["IMDB1"] || hostString == Operation.domains["IMDB2"])
                    {
                        hosts.Add(Operation.domains["IMDB1"], host);
                        hosts.Add(Operation.domains["IMDB2"], host);
                    }
                    else if (hostString == Operation.domains["FORBES1"] || hostString == Operation.domains["FORBES2"])
                    {
                        hosts.Add(Operation.domains["FORBES1"], host);
                        hosts.Add(Operation.domains["FORBES2"], host);
                    }
                    //else if (hostString == Operation.domains["BBC1"] || hostString == Operation.domains["BBC2"])
                    //{
                    //    hosts.Add(Operation.domains["BBC1"], host);
                    //    hosts.Add(Operation.domains["BBC2"], host);
                    //}
                    else if (hostString == Operation.domains["ESPN1"] || hostString == Operation.domains["ESPN2"])
                    {
                        hosts.Add(Operation.domains["ESPN1"], host);
                        hosts.Add(Operation.domains["ESPN2"], host);
                    }
                    else if (hostString == Operation.domains["WIKIPEDIA1"] || hostString == Operation.domains["WIKIPEDIA2"] || hostString == Operation.domains["WIKIPEDIA3"])
                    {
                        hosts.Add(Operation.domains["WIKIPEDIA1"], host);
                        hosts.Add(Operation.domains["WIKIPEDIA2"], host);
                        hosts.Add(Operation.domains["WIKIPEDIA3"], host);
                    }
                    else
                    {
                        hosts.Add(hostString, host);
                    }
                }

                return xmls;
            }
            catch (Exception e)
            {
                //some error for the robots.txt. Maybe not found??
                Console.WriteLine("Robots.txt error parsing for " + hostString + ". " + e.ToString());
                hosts.Add(hostString, host);
                return xmls;
            }

        }

        //parse xmls and add any relevant urls
        private int parseXml(string xml, string host, int counter)
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(@xml))
                {
                    XmlNodeType type;
                    bool isSiteMap = false;
                    bool isAllowed = true;
                    string loc = "";
                    string lastMod = null;
                    while (reader.Read())
                    {
                        type = reader.NodeType;
                        if (type == XmlNodeType.Element)
                        {
                            if (reader.Name == "sitemapindex")
                            {
                                isSiteMap = true;
                            }
                            else if (reader.Name == "loc")
                            {
                                if (reader.Read())
                                {
                                    isAllowed = true;
                                    loc = reader.Value.Trim();
                                    //loc end element
                                    if (reader.Read())
                                    {
                                        //check if last mod is next
                                        if (reader.Read())
                                        {
                                            if (reader.Name == "lastmod")
                                            {
                                                //get lastmod val
                                                if (reader.Read())
                                                {
                                                    lastMod = reader.Value.Trim();

                                                    //check cnn
                                                    if (host == Operation.domains["CNN1"] || host == Operation.domains["CNN2"])
                                                    {
                                                        string date = lastMod.Substring(0, 10); //format: 2017-02-17 
                                                        DateTime dateTime = Convert.ToDateTime(date);
                                                        int compare = DateTime.Compare(dateTime, Operation._CNN_DATE);
                                                        if (compare < 0)
                                                        {
                                                            isAllowed = false;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                lastMod = null;
                                            }
                                        }
                                    }

                                    if (isAllowed)
                                    {
                                        if (isSiteMap)
                                        {
                                            counter = parseXml(loc, host, counter);
                                        }
                                        else
                                        {
                                            //add url to url queue
                                            counter++;
                                            CloudQueueMessage urlMessage = new CloudQueueMessage(loc);
                                            urlQueue.AddMessage(urlMessage);
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error parsing xml " + e.ToString());
            }
            return counter;
        }
    }
}
