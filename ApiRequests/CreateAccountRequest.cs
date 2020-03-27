using wamsrv.ApiResponses;
using wamsrv.Database;
using wamsrv.Email;
using wamsrv.Security;

namespace wamsrv.ApiRequests
{
    public class CreateAccountRequest : ApiRequest
    {
        public readonly string Email;
        private readonly string Password;
        public CreateAccountRequest(ApiRequestId requestId, string email, string password)
        {
            RequestId = requestId;
            Email = email;
            Password = password;
        }
        public override void Process(ApiServer server)
        {
            if (server.AssertServerSetup(this) || server.AssertAccountNull())
            {
                server.UnitTesting.MethodSuccess = false;
                return;
            }
            if (!EmailEssentials.IsValid(Email))
            {
                ApiError.Throw(ApiErrorCode.InvalidEmailAddress, server, "Email address is invalid.");
                return;
            }
            bool success;
            using (DatabaseManager databaseManager = new DatabaseManager(server))
            {
                if(!databaseManager.CheckEmailAvailable(Email, out success))
                {
                    if (!success)
                    {
                        return;
                    }
                    ApiError.Throw(ApiErrorCode.InvalidEmailAddress, server, "Email address already in use.");
                    return;
                }
            }
            string passwordHash = SecurityManager.ScryptHash(Password);
            server.Account = new Account(new AccountInfo(null, null, null, null, null, null, null, null, null, null, null, null, null, 50, null, Email, true, true), false, string.Empty)
            {
                Password = passwordHash,
                AuthenticationCode = SecurityManager.GenerateSecurityCode(),
                AuthenticationId = ApiRequestId.ConfirmAccount,
                AuthenticationTime = DatabaseEssentials.GetTimeStamp()
            };
            EmailManager emailManager = EmailManager.Create(Subject.CreateAccount, Email, "new user", server.Account.AuthenticationCode);
            success = emailManager.Send();
            if (!success)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Failed to send confirmation email.");
                return;
            }
            GenericSuccessResponse apiResponse = new GenericSuccessResponse(ResponseId.CreateAccount, true);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(apiResponse);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}
