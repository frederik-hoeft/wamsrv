using wamsrv.ApiResponses;
using wamsrv.Database;
using wamsrv.Security;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;
using SerializedApiResponse = wamsrv.ApiResponses.SerializedApiResponse;

namespace wamsrv.ApiRequests
{
    public class UpdateAccountInfoRequest : ApiRequest
    {
        public readonly string Name;
        public readonly string Occupation;
        public readonly string Info;
        public readonly string Location;
        public readonly int Radius;
        public readonly bool IsVisible;
        public readonly bool ShowLog;
        public UpdateAccountInfoRequest(ApiRequestId requestId, string name, string occupation, string info, string location, int radius, bool isVisible, bool showLog)
        {
            RequestId = requestId;
            Name = name;
            Occupation = occupation;
            Info = info;
            Location = location;
            Radius = radius;
            IsVisible = isVisible;
            ShowLog = showLog;
        }
        public override void Process(ApiServer server)
        {
            if (server.AssertUserOffline(RequestId) || server.AssertIdNotSet(RequestId))
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager();
            string query;
            if (string.IsNullOrEmpty(server.Account.AccountInfo.UserId))
            {
                query = "SELECT hid FROM Tbl_user WHERE id = " + DatabaseEssentials.Security.Sanitize(server.Account.Id);
                SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
                SqlSingleOrDefaultResponse singleOrDefaultResponse = databaseManager.AwaitSingleOrDefaultResponse(sqlRequest);
                if (!singleOrDefaultResponse.Success)
                {
                    string errorCode = ApiError.Throw(ApiErrorCode.InternalServerError, RequestId, "Unable to determine userid.");
                    server.Network.Send(errorCode);
                    return;
                }
                server.Account.AccountInfo.UserId = singleOrDefaultResponse.Result;
            }
            AesContext aesContext = new AesContext(server.Account.AccountInfo.UserId);
            string cryptoName = aesContext.Encrypt(Name);
            string cryptoOccupation = aesContext.Encrypt(Occupation);
            string cryptoInfo = aesContext.Encrypt(Info);
            query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "UPDATE Tbl_user SET name = \"", cryptoName, "\", occupation = \"", cryptoOccupation, "\", info = \"", cryptoInfo, "\", location = \"", Location, "\", radius = ", Radius.ToString(), ", isVisible = ", IsVisible ? "1" : "0", ", showLog = ", ShowLog ? "1" : "0", ";" });
            SqlApiRequest sqlApiRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = databaseManager.AwaitModifyDataResponse(sqlApiRequest);
            if (!modifyDataResponse.Success)
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InternalServerError, RequestId, "Unable to update account info.");
                server.Network.Send(errorCode);
                return;
            }
            GenericSuccessResponse successResponse = new GenericSuccessResponse(ResponseId.UpdateAccountInfo, true);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(successResponse);
            string json = serializedApiResponse.Serialize();
            server.Network.Send(json);
        }
    }
}
