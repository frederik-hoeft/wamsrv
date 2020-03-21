using wamsrv.ApiResponses;
using wamsrv.Database;
using wamsrv.Email;
using wamsrv.Security;

namespace wamsrv.ApiRequests
{
    public class CreateAccountRequest : ApiRequest
    {
        public readonly string Email;
        public readonly string Password;
        public CreateAccountRequest(string email, string password)
        {
            Email = email;
            Password = password;
        }
        public override void Process(ApiServer server)
        {
            if (server.AssertAccountNotNull(RequestId))
            {
                return;
            }
            if (!EmailEssentials.IsValid(Email))
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InvalidEmailAddress, RequestId, "Email address is invalid.");
                server.Network.Send(errorCode);
                return;
            }
            string passwordHash = SecurityManager.ScryptHash(Password);
            server.Account = new Account(new AccountInfo(null, null, null, null, 50, null, Email, true, true), false, string.Empty)
            {
                Password = passwordHash,
                AuthenticationCode = SecurityManager.GenerateSecurityCode(),
                AuthenticationId = ApiRequestId.ActivateAccount,
                AuthenticationTime = DatabaseEssentials.GetTimeStamp()
            };
            EmailManager emailManager = EmailManager.Create(Subject.CreateAccount, Email, "new user", server.Account.AuthenticationCode);
            bool success = emailManager.Send();
            if (!success)
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InternalServerError, RequestId, "Failed to send confirmation email.");
                server.Network.Send(errorCode);
                return;
            }
            CreateAccountResponse apiResponse = new CreateAccountResponse(ResponseId.CreateAccount, true);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(apiResponse);
            string json = serializedApiResponse.Serialize();
            server.Network.Send(json);
        }
    }
}
