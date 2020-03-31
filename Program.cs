using System;
using System.Diagnostics;
using System.Threading;
using wamsrv.ApiRequests;
using wamsrv.Database;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv
{
    internal static class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        private static void Main()
        {
            MainServer.LoadConfig();
        }

        private static void PerformanceTest()
        {
            ApiServer server = ApiServer.CreateDummy();
            using DatabaseManager databaseManager = new DatabaseManager(server);
            const string query = "SELECT id FROM Tbl_user WHERE id = 1 LIMIT 1";
            SqlApiRequest request = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
            const int maxIter = 10000;
            Console.WriteLine("Performance Test: Sending " + maxIter.ToString() + " requests to DB server ...");
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < maxIter; i++)
            {
                SqlSingleOrDefaultResponse response = databaseManager.AwaitSingleOrDefaultResponse(request, out bool success);
                if (!success)
                {
                    break;
                }
                Console.WriteLine("Request #" + i.ToString() + " succeeded!");
            }
            stopwatch.Stop();
            double average = stopwatch.ElapsedMilliseconds / (double)maxIter;
            Console.WriteLine("Average time per request: " + average.ToString() + " ms.");
        }

        private static void LoadTest()
        {
            MainServer.LoadConfig();
            int threadCount = 0;
            while (true)
            {
                for (int i = 0; i < 250; i++)
                {
                    new Thread(() =>
                    {
                        while (true)
                        {
                            ApiServer server = ApiServer.CreateDummy();
                            server.Account = new Account
                            {
                                IsOnline = true
                            };
                            GetAccountInfoRequest request = new GetAccountInfoRequest(ApiRequestId.GetAccountInfo, "asdf");
                            request.Process(server);
                            Thread.Sleep(1000);
                        }
                    }).Start();
                    threadCount++;
                    Console.WriteLine("Simulating " + threadCount.ToString() + " users ...");
                }
                Thread.Sleep(30000);
            }
        }
    }
}