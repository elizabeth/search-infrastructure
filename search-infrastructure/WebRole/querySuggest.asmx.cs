using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using HybridTrie;
using Microsoft.WindowsAzure.Storage.Table;
using Elizabot;
using System;

namespace WebRole
{
    /// <summary>
    /// Summary description for querySuggest
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class querySuggest : System.Web.Services.WebService
    {
        private string path = Path.GetTempPath() + "\\wiki.txt";
        private int _maxMem = 20;
        private PerformanceCounter theMemCounter = new PerformanceCounter("Memory", "Available MBytes");
        private static Trie trie;
        private CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

        //download data from the blob
        [WebMethod]
        public string downloadWiki()
        {
            try
            {
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("blob");
                CloudBlockBlob blockBlob = container.GetBlockBlobReference("wiki.txt");

                // Save blob contents to a file.
                using (var fileStream = File.OpenWrite(path))
                {
                    blockBlob.DownloadToStream(fileStream);
                }

                return "Successfully downloaded data";
            }
            catch
            {
                return "Failed to download data";
            }
        }

        //builds trie
        [WebMethod]
        public string buildTrie()
        {
            trie = new Trie();
            int MAX = 1000;
            int count = 0;
            string term = "";

            try
            {
                if (!File.Exists(path))
                {
                    downloadWiki();
                }

                using (StreamReader sr = File.OpenText(path))
                {
                    while (!sr.EndOfStream)
                    {
                        term = sr.ReadLine();
                        count++;
                        trie.insert(term);

                        if (count % MAX == 0)
                        {
                            //perform memory check to ensure not running out of memory
                            if (getAvailableMBytes() <= _maxMem)
                            {
                                break;
                            }
                        }
                    }
                }

                try
                {
                    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                    CloudTable statsTable = tableClient.GetTableReference(Operation._STATS_TABLE);
                    statsTable.CreateIfNotExists();

                    StatEntity stat = new StatEntity();
                    stat.PartitionKey = Operation._TRIE_PKEY;
                    stat.RowKey = Operation._TRIE_RKEY;
                    stat.lastTenString = term;
                    stat.indexSize = count;
                    statsTable.ExecuteAsync(TableOperation.InsertOrReplace(stat));
                }
                catch (Exception e)
                {
                    return "Failed to insert trie stats into table";
                }

                return "Successfully built trie with " + count + " words. The last word was " + term;
            }
            catch (Exception e)
            {
                return "Error building trie";
            }
        }

        //takes in search term and returns results based on search
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string searchTrie(string term)
        {
            if (trie == null)
            {
                buildTrie();
            }

            return new JavaScriptSerializer().Serialize(trie.query(term));
        }

        //save user's search
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string saveSearch(string term)
        {
            if (trie == null)
            {
                buildTrie();
            }

            try
            {
                term = term.Trim().ToLower();

                trie.saveSearch(term);
                return new JavaScriptSerializer().Serialize("Search saved!");
            }
            catch (Exception e)
            {
                return new JavaScriptSerializer().Serialize("Error saving search");
            }
        }

        //get the available memory in mbytes
        private float getAvailableMBytes()
        {
            return theMemCounter.NextValue();
        }
    }
}
