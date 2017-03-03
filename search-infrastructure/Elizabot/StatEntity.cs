using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elizabot
{
    public class StatEntity : TableEntity
    {
        public string status { get; set; }
        public int cpu { get; set; }
        public int memory { get; set; }
        public string lastTenString { get; set; }
        public int urlsCrawled { get; set; }
        public int queueSize { get; set; }
        public int indexSize { get; set; }
        private Queue<Uri> lastTen;

        public StatEntity(string pkey, string rkey, float cpuNum, float memNum)
        {
            this.PartitionKey = Operation._STAT_PKEY;
            this.RowKey = Operation._STAT_RKEY;

            status = Operation._IDLE;
            cpu = (int)cpuNum;
            memory = (int)memNum;
            lastTenString = "";
            urlsCrawled = 0;
            queueSize = 0;
            indexSize = 0;
            lastTen = new Queue<Uri>();
        }

        public StatEntity() { }

        public void updateStatus(string status)
        {
            if (status == Operation._IDLE || status == Operation._LOADING || status == Operation._CRAWLING)
            {
                this.status = status;
            }
        }

        public void updateQueue()
        {
            updateQueueSize(1);
        }

        public void updateMachine(float cpu, float memory)
        {
            updateMachineCounters(cpu, memory);
        }

        //update various stats once a new url has been crawled
        public void updateStats(Uri uri, int num)
        {
            addLatest(uri);
            urlsCrawled++;
            queueSize--;
            updateQueueSize(num);
            indexSize++;
        }

        //update various stats if a url was crawled but did not update table with its data
        public void updateFailUrlStats()
        {
            urlsCrawled++;
            updateQueueRem();
        }

        //update various stats if a url was not allowed or already visited
        public void updateQueueRem()
        {
            updateQueueSize(-1);
        }

        public void updateQueue(int num)
        {
            updateQueueSize(num);
        }

        private void updateMachineCounters(float cpu, float memory)
        {
            this.cpu = (int)cpu;
            this.memory = (int)memory;
        }

        //add latest uri and remove oldest if more than 10
        private void addLatest(Uri uri)
        {
            if (lastTen.Count >= 10)
            {
                lastTen.Dequeue();
            }
            lastTen.Enqueue(uri);

            StringBuilder ten = new StringBuilder("");
            foreach (Uri each in lastTen)
            {
                ten.Append(each.AbsoluteUri + "; ");
            }
            lastTenString = ten.ToString();
        }

        private void updateQueueSize(int num)
        {
            queueSize += num;
        }
    }
}
