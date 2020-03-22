using System;
using wamsrv.ApiResponses;
using wamsrv.Database;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.ApiRequests
{
    public class CookieValidationRequest : ApiRequest
    {
        public readonly string Value;
        public CookieValidationRequest(ApiRequestId requestId, string value)
        {
            RequestId = requestId;
            Value = value;
        }
        public override void Process(ApiServer server)
        {
            server.RequestId = RequestId;
            if (server.AssertAccountNull())
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager(server);
            string query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "SELECT u.id, u.isOnline FROM Tbl_cookies as c, Tbl_user as u WHERE c.value = \'", Value, "\' AND c.expires > ", DatabaseEssentials.GetTimeStamp().ToString(), " AND u.id = c.userid;" });
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
            success = databaseManager.SetUserOnline(id);
            if (!success)
            {
                return;
            }
            if (server.Account == null)
            {
                server.Account = databaseManager.GetAccount(id, out SqlErrorState errorState);
                if (errorState != SqlErrorState.Success)
                {
                    if (errorState == SqlErrorState.GenericError)
                    {
                        ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to fetch account info.");
                    }
                    return;
                }
            }
            server.Account.IsOnline = true;
            CookieValidationResponse apiResponse = new CookieValidationResponse(ResponseId.CreateCookie, true);
            ApiResponses.SerializedApiResponse serializedApiResponse = ApiResponses.SerializedApiResponse.Create(apiResponse);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}
