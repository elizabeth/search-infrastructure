using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Elizabot;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System.Configuration;

namespace WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private static PerformanceCounter memCounter;
        private static PerformanceCounter cpuCounter;
        private static Crawler crawler;
        private static StatEntity stat;

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue opQueue = queueClient.GetQueueReference(Operation._OP_QUEUE);
            opQueue.CreateIfNotExists();
            CloudQueue robotQueue = queueClient.GetQueueReference(Operation._ROBOTS_QUEUE);
            robotQueue.CreateIfNotExists();
            CloudQueue urlQueue = queueClient.GetQueueReference(Operation._URLS_QUEUE);
            urlQueue.CreateIfNotExists();

            //Create table client
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            //Retrieve reference to pages table
            CloudTable pagesTable = tableClient.GetTableReference(Operation._PAGES_TABLE);
            pagesTable.CreateIfNotExists();
            //retrieve reference to stats table
            CloudTable statsTable = tableClient.GetTableReference(Operation._STATS_TABLE);
            statsTable.CreateIfNotExists();
            //retrieve reference to errors table
            CloudTable errorsTable = tableClient.GetTableReference(Operation._ERRORS_TABLE);
            errorsTable.CreateIfNotExists();

            crawler = new Crawler(robotQueue, urlQueue, pagesTable, statsTable, errorsTable);

            memCounter = new PerformanceCounter("Memory", "Available MBytes");
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            stat = new StatEntity(cpuCounter.NextValue(), memCounter.NextValue());
            updateStatsTable(statsTable);

            stat.updateRunning();

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(50);

                //check op queue for any commands
                CloudQueueMessage command = opQueue.GetMessage(TimeSpan.FromMinutes(5));
                while (command != null)
                {
                    string commandString = command.AsString;
                    //update status
                    stat.updateStatus(Operation._IDLE);
                    updateStatsTable(statsTable);

                    if (commandString == Operation._START)
                    {
                        if (!stat.getRunning())
                        {
                            stat.updateRunning();

                            pagesTable = tableClient.GetTableReference(Operation._PAGES_TABLE);
                            pagesTable.CreateIfNotExists();
                            errorsTable = tableClient.GetTableReference(Operation._ERRORS_TABLE);
                            errorsTable.CreateIfNotExists();
                        }
                    }
                    else if (commandString == Operation._CLEAR)
                    {
                        stat.updateRunning();

                        opQueue.Clear();
                        robotQueue.Clear();
                        urlQueue.Clear();

                        pagesTable.DeleteIfExists();
                        errorsTable.DeleteIfExists();

                        crawler = new Crawler(robotQueue, urlQueue, pagesTable, statsTable, errorsTable);
                        stat = new StatEntity(cpuCounter.NextValue(), memCounter.NextValue());
                        updateStatsTable(statsTable);

                        Thread.Sleep(40000);
                    }

                    updateStatsTable(statsTable);
                    opQueue.DeleteMessage(command);
                    command = opQueue.GetMessage(TimeSpan.FromMinutes(5));
                }

                if (stat.getRunning())
                {
                    //check robot queue for any robots to parse
                    CloudQueueMessage robotUrl = robotQueue.GetMessage(TimeSpan.FromMinutes(5));
                    while (robotUrl != null)
                    {
                        //update status
                        stat.updateStatus(Operation._LOADING);
                        updateStatsTable(statsTable);

                        int queueSizeUpdate = crawler.parseRobots(robotUrl.AsString);
                        //stat.updateQueue(queueSizeUpdate);
                        updateStatsTable(statsTable);

                        robotQueue.DeleteMessage(robotUrl);
                        robotUrl = robotQueue.GetMessage(TimeSpan.FromMinutes(5));
                    }

                    //check url queue for any urls
                    CloudQueueMessage queueUrl = urlQueue.GetMessage(TimeSpan.FromMinutes(5));
                    if (queueUrl != null)
                    {
                        //update status
                        stat.updateStatus(Operation._CRAWLING);
                        updateStatsTable(statsTable);

                        Tuple<int, int> crawled = crawler.crawlSite(queueUrl.AsString);
                        stat.updateStats(new Uri(queueUrl.AsString), crawled.Item1, crawled.Item2);
                        updateStatsTable(statsTable);

                        urlQueue.DeleteMessage(queueUrl);
                    }
                } else
                {
                    stat.updateStatus(Operation._IDLE);
                }

                updateStatsTable(statsTable);
                //Trace.TraceInformation("Working");
            }
        }

        private void updateStatsTable(CloudTable statsTable)
        {
            stat.updateMachine(cpuCounter.NextValue(), memCounter.NextValue());
            //Insert stat into table
            try
            {
                TableOperation insertOperation = TableOperation.InsertOrReplace(stat);
                statsTable.ExecuteAsync(insertOperation);
            }
            catch (Exception e)
            {
                Trace.TraceInformation(e.Message);
            }
        }
    }
}
