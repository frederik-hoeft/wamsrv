using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using wamsrv.ApiResponses;
using wamsrv.Database;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.ApiRequests
{
    public class CreateCookieRequest : ApiRequest
    {
        public readonly string Email;
        public readonly string Password;
        public readonly string Info;
        public CreateCookieRequest(ApiRequestId requestId, string email, string password, string info)
        {
            RequestId = requestId;
            Email = email;
            Password = password;
            Info = info;
        }
        public override void Process(ApiServer server)
        {
            using DatabaseManager databaseManager = new DatabaseManager() ;
            string query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "SELECT id, password, isOnline FROM Tbl_user WHERE email = \"", Email, "\" LIMIT 1;" });
            SqlApiRequest apiRequest = SqlApiRequest.Create(SqlRequestId.GetDataArray, query, 3);
            SqlDataArrayResponse dataArrayResponse = databaseManager.GetDataArrayResponse(apiRequest);
            string[] data = dataArrayResponse.Result;
            if (!dataArrayResponse.Success || data.Length != 3)
            {
                // TODO: return error message
                return;
            }
            string id = data[0];
            string hash = data[1];
            if (Convert.ToInt32(data[2]) == 1)
            {
                // TODO: return error message (user online on different device)
                return;
            }
            bool authenticationSuccessful = SecurityManager.ScryptCheck(Password, hash);
            if (!authenticationSuccessful)
            {
                // TODO: return error message
                return;
            }
            query = "UPDATE Tbl_data SET isOnline = 1 WHERE id = " + id.ToString();
            apiRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = databaseManager.GetModifyDataResponse(apiRequest);
            if (!modifyDataResponse.Success)
            {
                // TODO: return error message
                return;
            }
            string securityToken = SecurityManager.GenerateSecurityToken();
            // Token should expire every month.
            int expirationDate = DatabaseEssentials.GetTimeStamp() + 86400 * 30;
            query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "INSERT INTO Tbl_cookies (userid, value, expires, info) VALUES (", id, ",\"", securityToken, "\",", expirationDate.ToString(), ",\"", Info, "\");" });
            apiRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            modifyDataResponse = databaseManager.GetModifyDataResponse(apiRequest);
            if (!modifyDataResponse.Success)
            {
                // TODO: return error message
                return;
            }
            if (server.Account == null)
            {
                query = "SELECT hid, name, occupation, info, location, email, radius, isVisible, showLog FROM Tbl_user WHERE id = " + id.ToString() + " LIMIT 1;";
                apiRequest = SqlApiRequest.Create(SqlRequestId.GetDataArray, query, 9);
                dataArrayResponse = databaseManager.GetDataArrayResponse(apiRequest);
                string[] account = dataArrayResponse.Result;
                if (!dataArrayResponse.Success || account.Length != 9)
                {
                    // TODO: return error message
                    return;
                }
                string userid = account[0];
                string userSecret = SecurityManager.DeriveUserSecret(userid);
                string name = SecurityManager.AESDecrypt(account[1], userSecret);
                string occupation = SecurityManager.AESDecrypt(account[2], userSecret);
                string info = SecurityManager.AESDecrypt(account[3], userSecret);
                string location = SecurityManager.AESDecrypt(account[4], userSecret);
                string email = account[5];
                bool successParse1 = int.TryParse(account[6], out int radius);
                bool successParse2 = int.TryParse(account[7], out int isVisible);
                bool successParse3 = int.TryParse(account[8], out int showLog);
                if (!successParse1 || !successParse2 || !successParse3)
                {
                    // TODO: return error message
                    return;
                }
                AccountInfo accountInfo = new AccountInfo(name, occupation, info, location, radius, userid, email, Convert.ToBoolean(isVisible), Convert.ToBoolean(showLog));
                server.Account = new Account(accountInfo, false);
            }
            CreateCookieResponse apiResponse = new CreateCookieResponse(ResponseId.CreateCookie, securityToken);
            ApiResponses.SerializedApiResponse serializedApiResponse = ApiResponses.SerializedApiResponse.Create(apiResponse);
            string json = serializedApiResponse.Serialize();
            server.Network.Send(json);
        }
    }
}
