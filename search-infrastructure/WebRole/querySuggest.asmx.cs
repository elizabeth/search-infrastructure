using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using HybridTrie;

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

        //download data from the blob
        [WebMethod]
        public string downloadWiki()
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);
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
            int curr = 0;

            try
            {
                if (!File.Exists(path))
                {
                    downloadWiki();
                }

                using (StreamReader sr = File.OpenText(path))
                {
                    string term = "";
                    while ((term = sr.ReadLine()) != null)
                    {
                        curr++;
                        trie.insert(term);

                        if (curr >= MAX)
                        {
                            //perform memory check to ensure not running out of memory
                            if (getAvailableMBytes() <= _maxMem)
                            {
                                break;
                            }
                            curr = 0;
                        }
                    }
                }

                return "Successfully built trie";
            }
            catch
            {
                return "Failed to build trie";
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
                trie.saveSearch(term);
                return new JavaScriptSerializer().Serialize("No search results found!");
            }
            catch
            {
                return new JavaScriptSerializer().Serialize("Error searching");
            }
        }

        //get the available memory in mbytes
        private float getAvailableMBytes()
        {
            return theMemCounter.NextValue();
        }
    }
}
