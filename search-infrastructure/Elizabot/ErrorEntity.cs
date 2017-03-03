using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elizabot
{
    public class ErrorEntity : TableEntity
    {
        public string url { get; set; }
        public string err { get; set; }
        public ErrorEntity(string url, string err, string date)
        {
            this.PartitionKey = Operation.md5Hash(url);
            this.RowKey = Guid.NewGuid().ToString();

            this.url = url;
            this.err = err;
        }

        public ErrorEntity() { }
    }
}
