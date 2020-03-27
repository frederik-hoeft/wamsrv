using System;
using System.Collections.Generic;
using System.Text;
using wamsrv.ApiResponses;
using wamsrv.Database;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.ApiRequests
{
    public class ConfirmPasswordChangeRequest : ApiRequest
    {
        public readonly string Code;
        public readonly string SecurityToken;
        public ConfirmPasswordChangeRequest(ApiRequestId requestId, string code, string securityToken)
        {
            RequestId = requestId;
            Code = code;
            SecurityToken = securityToken;
        }
        public override void Process(ApiServer server)
        {
            if (server == null)
            {
                return;
            }
            server.RequestId = RequestId;
            if (server.AssertAuthenticationCodeInvalid(Code) || server.AssertUserOnline() || server.AssertPasswordSet() || server.AssertIdSet())
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager(server);
            // Check if security token is valid.
            string query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "SELECT u.id FROM Tbl_cookies as c, Tbl_user as u WHERE c.value = \'", SecurityToken, "\' AND u.id = c.userid;" });
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 2);
            SqlSingleOrDefaultResponse singleOrDefaultResponse = databaseManager.AwaitSingleOrDefaultResponse(sqlRequest, out bool success);
            if (!success)
            {
                return;
            }
            if (!singleOrDefaultResponse.Success || !singleOrDefaultResponse.Result.Equals(server.Account.Id))
            {
                ApiError.Throw(ApiErrorCode.InvalidToken, server, "Security token was invalid.");
                return;
            }
            // Reset security token expiration timer..
            int expirationDate = DatabaseEssentials.GetTimeStamp() + MainServer.Config.WamsrvSecurityConfig.SecurityTokenExpirationTime;
            query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "UPDATE Tbl_cookies SET expires = \'", expirationDate.ToString(), "\' WHERE value = \'", SecurityToken, "\';" });
            sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = databaseManager.AwaitModifyDataResponse(sqlRequest, out success);
            if (!success)
            {
                return;
            }
            if (!modifyDataResponse.Success)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to refresh security token.");
                return;
            }
            // Delete all other security tokens associated with the account.
            if (databaseManager.DeleteSecurityTokens(new string[] { SecurityToken }))
            {
                return;
            }
            // Update password.
            if (databaseManager.UpdatePassword())
            {
                return;
            }
            server.Account.AuthenticationCode = string.Empty;
            server.Account.AuthenticationId = ApiRequestId.Invalid;
            server.Account.AuthenticationTime = -1;
            GenericSuccessResponse response = new GenericSuccessResponse(ResponseId.ConfirmAccount, true);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(response);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}
