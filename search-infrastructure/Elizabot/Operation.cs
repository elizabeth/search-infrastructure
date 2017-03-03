using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Elizabot
{
    public class Operation
    {
        public static string _START = "START";
        public static string _CLEAR = "CLEAR";
        public static string _IDLE = "idle";            //status 
        public static string _LOADING = "loading";      //status
        public static string _CRAWLING = "crawling";    //status
        public static string _OP_QUEUE = "operations";
        public static string _ROBOTS_QUEUE = "robots";
        public static string _URLS_QUEUE = "urls";
        public static string _PAGES_TABLE = "pages";
        public static string _ERRORS_TABLE = "errors";
        public static string _STATS_TABLE = "stats";
        public static string _STAT_PKEY = "stats";
        public static string _STAT_RKEY = "super-stats";
        public static string _TRIE_PKEY = "trie";
        public static string _TRIE_RKEY = "super-trie";
        public static string _CLRPW = "421458b04cdb19cc25c0f44a52c077c7";
        public static DateTime _CNN_DATE = new DateTime(2017, 01, 01);  //cnn domain cutoff date
        public static string _BR_SITEMAP = "http://bleacherreport.com/sitemap/nba.xml"; //bleacherreport sitemap to follow
        //public static string _BBC_SITEMAP = "http://www.bbc.co.uk/sitemap.xml";
        public static Dictionary<string, string> domains
            = new Dictionary<string, string>
            {
                { "CNN1", "cnn.com" },
                { "CNN2", "www.cnn.com" },
                { "BR1", "bleacherreport.com" },
                { "BR2", "www.bleacherreport.com" },
                { "IMDB1", "imdb.com" },
                { "IMDB2", "www.imdb.com" },
                { "FORBES1", "forbes.com" },
                { "FORBES2", "www.forbes.com" },
                //{ "BBC1", "bbc.co.uk" },
                //{ "BBC2", "www.bbc.co.uk" },
                { "ESPN1", "espn.com" },
                { "ESPN2", "www.espn.com" },
                { "WIKIPEDIA1", "wikipedia.org" },
                { "WIKIPEDIA2", "en.wikipedia.org" },
                { "WIKIPEDIA3", "www.wikipedia.org" },

            };

        public static string md5Hash(string toHash)
        {
            byte[] ended = new UTF8Encoding().GetBytes(toHash);

            MD5 md5Hash = MD5.Create();

            // need MD5 to calculate the hash
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(ended);

            // string representation (similar to UNIX format)
            string hashed = BitConverter.ToString(hash)
               // without dashes
               .Replace("-", string.Empty)
               // make lowercase
               .ToLower();

            return hashed;
        }

        //html body parsing
        //from https://gist.github.com/frankhale/3240804
        public static string stripHtml(string value)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(value);

            if (htmlDoc == null)
                return value;

            StringBuilder sanitizedString = new StringBuilder();

            foreach (var node in htmlDoc.DocumentNode.ChildNodes)
                sanitizedString.Append(node.InnerText);

            return sanitizedString.ToString();
        }
    }
}
