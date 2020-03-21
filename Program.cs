using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using wamsrv.ApiRequests;
using wamsrv.ApiResponses;
using wamsrv.Database;
using washared;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv
{
    class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            MainServer.LoadConfig();
            string hash = SecurityManager.GenerateSecurityToken();
            Debug.WriteLine("Generated '" + hash + "'");
            return;
            Test();
        }

        static void Test()
        {
            using (DatabaseManager databaseManager = new DatabaseManager())
            {
                SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, "INSERT INTO Tbl_user (password, name, hid, email) VALUES ('password1234','user3210','7', 'mail7@example.com');", -1);
                SqlModifyDataResponse modifyDataResponse = databaseManager.GetModifyDataResponse(sqlRequest);
                Debug.WriteLine(modifyDataResponse.Result);
                Thread.Sleep(15000);
                sqlRequest = SqlApiRequest.Create(SqlRequestId.Get2DArray, "SELECT * FROM Tbl_user;", 12);
                Sql2DArrayResponse arrayResponse = databaseManager.Get2DArrayResponse(sqlRequest);
                if (!arrayResponse.Success)
                {
                    Debug.WriteLine("Unsuccessful :C");
                }
                for (int i = 0; i < arrayResponse.Result.Length; i++)
                {
                    for (int j = 0; j < arrayResponse.Result[i].Length; j++)
                    {
                        Debug.Write(", " +  arrayResponse.Result[i][j]);
                    }
                    Debug.WriteLine("");
                }
                Debug.WriteLine(arrayResponse.Result);
            }
            Debug.WriteLine("Done :)");
        }
    }
}
