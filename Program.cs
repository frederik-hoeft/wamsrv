using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using wamsrv.ApiRequests;
using wamsrv.Database;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv
{
    class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        static void Main()
        {
            MainServer.LoadConfig();
            using DatabaseManager databaseManager = new DatabaseManager(ApiServer.CreateDummy());
            string query = "INSERT INTO Tbl_log (userid, value) VALUES (17, \'asdf\'); UPDATE Tbl_log SET value = \'fdsa\' WHERE userid = 17;";
            SqlApiRequest sqlApiRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            _ = databaseManager.AwaitModifyDataResponse(sqlApiRequest, out _);
        }

        static void PerformanceTest()
        {
            ApiServer server = ApiServer.CreateDummy();
            using DatabaseManager databaseManager = new DatabaseManager(server);
            string query = "SELECT id FROM Tbl_user WHERE id = 1 LIMIT 1";
            SqlApiRequest request = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
            int maxIter = 10000;
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

        static void LoadTest()
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
