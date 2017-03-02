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
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
        private static CloudQueueClient queueClient;
        private static CloudQueue opQueue;
        private static CloudQueue robotQueue;
        private static CloudQueue urlQueue;
        private static CloudTableClient tableClient;
        private static CloudTable pagesTable;
        private static CloudTable errorsTable;
        private static CloudTable statsTable;
        private static Dictionary<string, List<string>> searchcache;

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string searchPages(string term)
        {
            term = term.Trim().ToLower();

            //search table and return relevant pages
            try
            {
         
                return new JavaScriptSerializer().Serialize("");
            }
            catch (Exception e)
            {
                return "Error retrieving results.";
            }

        }
    }
}
