using System;
using System.Collections.Generic;
using System.Text;
using wamsrv.ApiResponses;
using wamsrv.Database;
using wamsrv.Security;

namespace wamsrv.ApiRequests
{
    public class ConfirmPasswordResetRequest : ApiRequest
    {
        public readonly string Code;
        private readonly string Password;
        public ConfirmPasswordResetRequest(ApiRequestId requestId, string code, string password)
        {
            RequestId = requestId;
            Code = code;
            Password = password;
        }
        public override void Process(ApiServer server)
        {
            if (server == null)
            {
                return;
            }
            server.RequestId = RequestId;
            if (server.AssertAuthenticationCodeInvalid(Code) || server.AssertUserOffline() || server.AssertIdSet())
            {
                return;
            }
            server.Account.Password = SecurityManager.ScryptHash(Password);
            using DatabaseManager databaseManager = new DatabaseManager(server);
            if (databaseManager.UpdatePassword() || databaseManager.DeleteSecurityTokens(Array.Empty<string>()))
            {
                return;
            }
            GenericSuccessResponse response = new GenericSuccessResponse(ResponseId.ConfirmPasswordReset, true);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(response);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.Account = null;
            server.UnitTesting.MethodSuccess = true;
        }
    }
}
