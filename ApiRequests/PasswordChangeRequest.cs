using System;
using System.Collections.Generic;
using System.Text;
using wamsrv.ApiResponses;
using wamsrv.Database;
using wamsrv.Email;
using wamsrv.Security;

namespace wamsrv.ApiRequests
{
    public class PasswordChangeRequest : ApiRequest
    {
        private readonly string Password;

        public PasswordChangeRequest(ApiRequestId requestId, string password)
        {
            RequestId = requestId;
            Password = password;
        }
        public override void Process(ApiServer server)
        {
            if (server.AssertServerSetup(this) || server.AssertAccountNotNull() || server.AssertUserOnline() || server.AssertEmailSet())
            {
                return;
            }
            if (!EmailEssentials.IsValid(server.Account.AccountInfo.Email))
            {
                ApiError.Throw(ApiErrorCode.InvalidEmailAddress, server, "Email address is invalid.");
                return;
            }
            server.Account.AuthenticationCode = SecurityManager.GenerateSecurityCode();
            server.Account.AuthenticationId = ApiRequestId.ConfirmPasswordChange;
            server.Account.AuthenticationTime = DatabaseEssentials.GetTimeStamp();
            server.Account.Password = SecurityManager.ScryptHash(Password);
            string name = string.IsNullOrEmpty(server.Account.AccountInfo.Name) ? "user" : server.Account.AccountInfo.Name;
            EmailManager emailManager = EmailManager.Create(Subject.ChangePassword, server.Account.AccountInfo.Email, name, server.Account.AuthenticationCode);
            bool success = emailManager.Send();
            if (!success)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Failed to send confirmation email.");
                return;
            }
            GenericSuccessResponse apiResponse = new GenericSuccessResponse(ResponseId.PasswordChange, true);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(apiResponse);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}
