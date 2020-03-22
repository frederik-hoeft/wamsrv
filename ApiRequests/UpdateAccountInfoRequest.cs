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
            server.RequestId = RequestId;
            if (server.AssertUserOnline() || server.AssertIdSet())
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager(server);
            string query;
            bool success;
            if (string.IsNullOrEmpty(server.Account.AccountInfo.UserId))
            {
                query = "SELECT hid FROM Tbl_user WHERE id = " + DatabaseEssentials.Security.Sanitize(server.Account.Id);
                SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
                SqlSingleOrDefaultResponse singleOrDefaultResponse = databaseManager.AwaitSingleOrDefaultResponse(sqlRequest, out success);
                if (!success)
                {
                    return;
                }
                if (!singleOrDefaultResponse.Success)
                {
                    ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to determine userid.");
                    return;
                }
                server.Account.AccountInfo.UserId = singleOrDefaultResponse.Result;
            }
            AesContext aesContext = new AesContext(server.Account.AccountInfo.UserId);
            string cryptoName = aesContext.EncryptOrDefault(Name);
            string cryptoOccupation = aesContext.EncryptOrDefault(Occupation);
            string cryptoInfo = aesContext.EncryptOrDefault(Info);
            query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "UPDATE Tbl_user SET name = \"", cryptoName, "\", occupation = \"", cryptoOccupation, "\", info = \"", cryptoInfo, "\", location = \"", Location, "\", radius = ", Radius.ToString(), ", isVisible = ", IsVisible ? "1" : "0", ", showLog = ", ShowLog ? "1" : "0", ";" });
            SqlApiRequest sqlApiRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = databaseManager.AwaitModifyDataResponse(sqlApiRequest, out success);
            if (!success)
            {
                return;
            }
            if (!modifyDataResponse.Success)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to update account info.");
                return;
            }
            GenericSuccessResponse successResponse = new GenericSuccessResponse(ResponseId.UpdateAccountInfo, true);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(successResponse);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
        }
    }
}
