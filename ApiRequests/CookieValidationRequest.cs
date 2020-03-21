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
            if (server.AssertAccountNotNull(RequestId))
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager();
            string query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "SELECT userid, isOnline FROM Tbl_cookies WHERE value = \"", Value, "\" AND expires > ", DatabaseEssentials.GetTimeStamp().ToString(), ";" });
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetDataArray, query, 2);
            SqlDataArrayResponse dataArrayResponse = databaseManager.AwaitDataArrayResponse(sqlRequest);
            if (!dataArrayResponse.Success || dataArrayResponse.Result.Length != 2)
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InvalidToken, RequestId, "Security token expired or invalid.");
                server.Network.Send(errorCode);
                return;
            }
            string id = dataArrayResponse.Result[0];
            string isOnline = dataArrayResponse.Result[1];
            if (Convert.ToInt32(isOnline) == 1)
            {
                string errorCode = ApiError.Throw(ApiErrorCode.AlreadyOnline, RequestId, "Already logged in from another device.");
                server.Network.Send(errorCode);
                return;
            }
            bool success = databaseManager.SetUserOnline(id);
            if (!success)
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InternalServerError, RequestId, "Unable to change online status.");
                server.Network.Send(errorCode);
                return;
            }
            if (server.Account == null)
            {
                server.Account = databaseManager.GetAccount(id, out success);
                if (!success)
                {
                    string errorCode = ApiError.Throw(ApiErrorCode.InternalServerError, RequestId, "Unable to fetch account info.");
                    server.Network.Send(errorCode);
                    return;
                }
            }
            server.Account.IsOnline = true;
            CookieValidationResponse apiResponse = new CookieValidationResponse(ResponseId.CreateCookie, true);
            ApiResponses.SerializedApiResponse serializedApiResponse = ApiResponses.SerializedApiResponse.Create(apiResponse);
            string json = serializedApiResponse.Serialize();
            server.Network.Send(json);
        }
    }
}
