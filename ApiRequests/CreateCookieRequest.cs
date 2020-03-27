using System;
using wamsrv.ApiResponses;
using wamsrv.Database;
using wamsrv.Security;
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
            if (server.AssertServerSetup(this) || server.AssertAccountNull())
            {
                server.UnitTesting.MethodSuccess = false;
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager(server);
            string query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "SELECT id, password, isOnline FROM Tbl_user WHERE email = \'", Email, "\' LIMIT 1;" });
            SqlApiRequest apiRequest = SqlApiRequest.Create(SqlRequestId.GetDataArray, query, 3);
            SqlDataArrayResponse dataArrayResponse = databaseManager.AwaitDataArrayResponse(apiRequest, out bool success);
            string[] data = dataArrayResponse.Result;
            if (!success)
            {
                return;
            }
            if (!dataArrayResponse.Success || data.Length != 3)
            {
                ApiError.Throw(ApiErrorCode.InvalidUser, server, "No account is associated with this email address.");
                return;
            }
            string id = data[0];
            string hash = data[1];
            if (Convert.ToInt32(data[2]) == 1)
            {
                ApiError.Throw(ApiErrorCode.AlreadyOnline, server, "Already logged in from another device.");
                return;
            }
            bool authenticationSuccessful = SecurityManager.ScryptCheck(Password, hash);
            if (!authenticationSuccessful)
            {
                ApiError.Throw(ApiErrorCode.InvalidCredentials, server, "Incorrect password.");
                return;
            }
            string securityToken = SecurityManager.GenerateSecurityToken();
            // Token should expire every month.
            int expirationDate = DatabaseEssentials.GetTimeStamp() + MainServer.Config.WamsrvSecurityConfig.SecurityTokenExpirationTime;
            query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "INSERT INTO Tbl_cookies (userid, value, expires, info) VALUES (", id, ",\'", securityToken, "\',", expirationDate.ToString(), ",\'", Info, "\');" });
            apiRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = databaseManager.AwaitModifyDataResponse(apiRequest, out success);
            if (!success)
            {
                return;
            }
            if (!modifyDataResponse.Success)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to generate security token.");
                return;
            }
            if (server.Account == null)
            {
                server.Account = databaseManager.GetAccount(id, out SqlErrorState sqlErrorState);
                if (sqlErrorState != SqlErrorState.Success)
                {
                    if (sqlErrorState == SqlErrorState.GenericError)
                    {
                        ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to fetch account info.");
                    }
                    return;
                }
            }
            success = databaseManager.ApplyPermissions();
            if (!success)
            {
                return;
            }
            success = databaseManager.SetUserOnline();
            if (!success)
            {
                return;
            }
            CreateCookieResponse apiResponse = new CreateCookieResponse(ResponseId.CreateCookie, securityToken, server.Account.Permissions);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(apiResponse);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}
