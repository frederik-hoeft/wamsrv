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
            Debug.WriteLine(((int)Permission.QUERY_STATISTICS).ToString());
            return;
            LoadTest();
            return;
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
