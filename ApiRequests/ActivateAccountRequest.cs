using wamsrv.ApiResponses;
using wamsrv.Database;
using wamsrv.Security;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;
using SerializedApiResponse = wamsrv.ApiResponses.SerializedApiResponse;

namespace wamsrv.ApiRequests
{
    public class ActivateAccountRequest : ApiRequest
    {
        public readonly string Code;
        public ActivateAccountRequest(ApiRequestId requestId, string code)
        {
            RequestId = requestId;
            Code = code;
        }
        public override void Process(ApiServer server)
        {
            if (server.AssertAuthenticationCode(RequestId, Code) || server.AssertUserOnline(RequestId))
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager();
            string userid = SecurityManager.GenerateUserId();
            string query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "INSERT INTO Tbl_user (password, hid, email) VALUES (\"", server.Account.Password, "\",\"", userid, "\", \"", server.Account.AccountInfo.Email, "\");" });
            SqlApiRequest sqlRequets = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = databaseManager.AwaitModifyDataResponse(sqlRequets);
            if (!modifyDataResponse.Success)
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InternalServerError, RequestId, "Unable to create user.");
                server.Network.Send(errorCode);
                return;
            }
            server.Account.AuthenticationCode = string.Empty;
            server.Account.AuthenticationId = ApiRequestId.Invalid;
            server.Account.AuthenticationTime = -1;
            GenericSuccessResponse response = new GenericSuccessResponse(ResponseId.ActivateAccount, true);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(response);
            string json = serializedApiResponse.Serialize();
            server.Network.Send(json);
        }
    }
}
