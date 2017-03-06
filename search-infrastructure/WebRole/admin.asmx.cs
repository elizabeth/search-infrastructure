using Elizabot;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;

namespace WebRole
{
    /// <summary>
    /// Summary description for admin
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class admin : System.Web.Services.WebService
    {
        private CloudQueueClient queueClient;
        private CloudQueue opQueue;
        private CloudQueue robotQueue;
        //private CloudQueue urlQueue;
        private CloudTableClient tableClient;
        private CloudTable pagesTable;
        private CloudTable errorsTable;
        private CloudTable statsTable;
        private static Dictionary<string, List<PagePair>> searchCache;
        private CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

        //start crawling given root url
        [WebMethod]
        public string startCrawling(string url)
        {
            //parse input
            url = url.Trim().ToLower();

            opQueue = setQueue(Operation._OP_QUEUE);
            robotQueue = setQueue(Operation._ROBOTS_QUEUE);

            //check given url valid and is within allowed domains
            try
            {
                if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    url = "http://" + url;
                }

                Uri uri = new Uri(url, UriKind.Absolute);
                string uriString = uri.AbsoluteUri;

                if (Operation.domains.Values.Any(uri.Host.EndsWith))
                {
                    CloudQueueMessage opMessage = new CloudQueueMessage(Operation._START);
                    opQueue.AddMessage(opMessage);

                    //add url
                    CloudQueueMessage robotMessage = new CloudQueueMessage(uriString);
                    robotQueue.AddMessage(robotMessage);

                    return "Started crawling " + uriString;
                }
                else
                {
                    return "Url is not in the allowed domain";
                }
            }
            catch (Exception e)
            {
                return "Please enter a valid url";
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string searchPages(string term)
        {
            term = Operation.stripPunct(term.ToLower());
            pagesTable = setTable(Operation._PAGES_TABLE);

            //check cache first if there
            if (searchCache == null || searchCache.Count > 100)
            {
                searchCache = new Dictionary<string, List<PagePair>>();
            }

            if (searchCache.ContainsKey(term))
            {
                return new JavaScriptSerializer().Serialize(searchCache[term]);
            }

            //search table and return relevant page results ranked
            try
            {
                Dictionary<string, PagePair> results = new Dictionary<string, PagePair>();
                string[] terms = term.Split();
                foreach (string word in terms)
                {
                    TableQuery<PageEntity> pageQuery = new TableQuery<PageEntity>()
                        .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, word));
                    List<PageEntity> pages = pagesTable.ExecuteQuery(pageQuery).ToList();

                    if (pages.Count != 0)
                    {
                        foreach (PageEntity page in pages)
                        {
                            if (results.ContainsKey(page.uri))
                            {
                                results[page.uri].count += 1;
                                results[page.uri].queryWords.Add(word);
                            } else
                            {
                                results.Add(page.uri, new PagePair(page, 1, word));
                            }
                        }
                    }
                }

                //Rank based on number of keyword matches and then by date
                List<PagePair> searchResults = results.OrderByDescending(x => x.Value.count)
                    .ThenByDescending(x => x.Value.page.pubDate)
                    .Take(20)
                    .Select(x => x.Value)
                    .ToList();

                //add to cache
                searchCache.Add(term, searchResults);

                return new JavaScriptSerializer().Serialize(searchResults);
            }
            catch (Exception e)
            {
                return "Error retrieving results";
            }
        }

        //get return page title of the given url
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string getPageTitle(string url)
        {
            //parse url
            url = url.Trim();

            pagesTable = setTable(Operation._PAGES_TABLE);

            //check given url valid
            try
            {
                if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    url = "http://" + url;
                }

                Uri uri = new Uri(url, UriKind.Absolute);
                string uriString = Operation.md5Hash(uri.AbsoluteUri);
                try
                {
                    //Create the table query
                    TableQuery<PageEntity> pageQuery = new TableQuery<PageEntity>()
                        .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, uriString));
                    List<PageEntity> pages = pagesTable.ExecuteQuery(pageQuery).ToList();

                    return pages.Count != 0 ? pages.First().title : "Title not found";
                }
                catch (Exception e)
                {
                    return "Error retrieving title";
                }

            }
            catch (Exception e)
            {
                return "Please enter a valid url";
            }
        }

        //clear index and stop crawling
        [WebMethod]
        public string clearIndex(string password)
        {
            password = password.Trim();
            if (Operation.md5Hash(password) == Operation._CLRPW)
            {
                opQueue = setQueue(Operation._OP_QUEUE);
                try
                {
                    CloudQueueMessage opMessage = new CloudQueueMessage(Operation._CLEAR);
                    opQueue.AddMessage(opMessage);
                    return "Stopping crawl... Clearing index...";
                }
                catch (Exception e)
                {
                    return "Error clearing index";
                }
            }
            else
            {
                return "Incorrect password";
            }
        }

        //get and return state of each worker, machine counters, last 10 urls crawled
        //get and return number of urls crawled, url queue size, index size
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string getStats()
        {
            statsTable = setTable(Operation._STATS_TABLE);

            try
            {
                //get stats from stats table
                //Create the table query
                TableQuery<StatEntity> statQuery = new TableQuery<StatEntity>()
                    .Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, Operation._STAT_PKEY),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, Operation._STAT_RKEY)));
                StatEntity stat = statsTable.ExecuteQuery(statQuery).First();
                
                return new JavaScriptSerializer().Serialize(stat);
            }
            catch (Exception e)
            {
                return "Error retrieving stats";
            }
        }

        //get and return # of words in trie and last word inserted
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string getTrieStats()
        {
            statsTable = setTable(Operation._STATS_TABLE);

            try
            {
                TableQuery<StatEntity> statQuery = new TableQuery<StatEntity>()
                    .Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, Operation._TRIE_PKEY),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, Operation._TRIE_RKEY)));
                StatEntity stat = statsTable.ExecuteQuery(statQuery).First();

                return new JavaScriptSerializer().Serialize(stat);
            }
            catch (Exception e)
            {
                return "Error retrieving trie stats";
            }


        }

        //get and return any errors
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string getErrors()
        {
            errorsTable = setTable(Operation._ERRORS_TABLE);

            List<ErrorEntity> errorsList = new List<ErrorEntity>();

            try
            {
                //get errors from errors table
                //Create the table query
                TableQuery<ErrorEntity> errorQuery = new TableQuery<ErrorEntity>();
                List<ErrorEntity> errors = errorsTable.ExecuteQuery(errorQuery).ToList();

                //limit the errors returned to 20
                foreach (ErrorEntity error in errors)
                {
                    if (errorsList.Count < 20)
                    {
                        errorsList.Add(error);
                    } else
                    {
                        break;
                    }
                }

                return new JavaScriptSerializer().Serialize(errorsList);
            }
            catch (Exception e)
            {
                return "Error retrieving errors";
            }
        }

        private CloudQueue setQueue(string queue)
        {
            queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue cloudQueue = queueClient.GetQueueReference(queue);
            cloudQueue.CreateIfNotExists();

            return cloudQueue;
        }

        private CloudTable setTable(string table)
        {
            tableClient = storageAccount.CreateCloudTableClient();
            CloudTable cloudTable = tableClient.GetTableReference(table);
            cloudTable.CreateIfNotExists();

            return cloudTable;
        }
    }
}
