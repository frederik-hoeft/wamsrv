using System;
using wamsrv.ApiResponses;
using wamsrv.Database;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.ApiRequests
{
    public class CookieValidationRequest : ApiRequest
    {
        public readonly string SecurityToken;

        public CookieValidationRequest(ApiRequestId requestId, string securityToken)
        {
            RequestId = requestId;
            SecurityToken = securityToken;
        }

        public override void Process(ApiServer server)
        {
            if (server.AssertServerSetup(this) || server.AssertAccountNull())
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager(server);
            string query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "SELECT u.id, u.isOnline FROM Tbl_cookies as c, Tbl_user as u WHERE c.value = \'", SecurityToken, "\' AND c.expires > ", DatabaseEssentials.GetTimeStamp().ToString(), " AND u.id = c.userid;" });
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetDataArray, query, 2);
            SqlDataArrayResponse dataArrayResponse = databaseManager.AwaitDataArrayResponse(sqlRequest, out bool success);
            if (!success)
            {
                return;
            }
            if (!dataArrayResponse.Success || dataArrayResponse.Result.Length != 2)
            {
                ApiError.Throw(ApiErrorCode.InvalidToken, server, "Security token expired or invalid.");
                return;
            }
            string id = dataArrayResponse.Result[0];
            string isOnline = dataArrayResponse.Result[1];
            if (Convert.ToInt32(isOnline) == 1)
            {
                ApiError.Throw(ApiErrorCode.AlreadyOnline, server, "Already logged in from another device.");
                return;
            }
            if (!databaseManager.SetupAccount(id))
            {
                return;
            }
            Permission permissions = databaseManager.GetUserPermission(server.Account.AccountInfo.UserId, out success);
            if (!success)
            {
                return;
            }
            CookieValidationResponse apiResponse = new CookieValidationResponse(ResponseId.CreateCookie, true, permissions);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(apiResponse);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}