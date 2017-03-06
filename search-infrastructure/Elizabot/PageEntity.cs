using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elizabot
{
    public class PageEntity : TableEntity
    {
        public string uri { get; set; }
        public string title { get; set; }
        public string pubDate { get; set; }
        public string desc { get; set; }

        public PageEntity(Uri uri, string title, string pubDate, string desc, string keyWord)
        {
            this.PartitionKey = keyWord;
            this.RowKey = Operation.md5Hash(uri.AbsoluteUri);

            this.uri = uri.AbsoluteUri;
            this.title = title;
            this.pubDate = pubDate;
            this.desc = desc;
        }

        public PageEntity() { }
    }
}
