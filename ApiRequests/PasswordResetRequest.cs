using wamsrv.ApiResponses;
using wamsrv.Database;
using wamsrv.Email;
using wamsrv.Security;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.ApiRequests
{
    public class PasswordResetRequest : ApiRequest
    {
        public readonly string Email;

        public PasswordResetRequest(ApiRequestId requestId, string email)
        {
            RequestId = requestId;
            Email = email;
        }

        public override void Process(ApiServer server)
        {
            if (server.AssertServerSetup(this) || server.AssertAccountNull())
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager(server);
            string query = "SELECT isOnline, name, hid, id FROM Tbl_user WHERE email = \'" + DatabaseEssentials.Security.Sanitize(Email) + "\';";
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetDataArray, query, 4);
            SqlDataArrayResponse dataArrayResponse = databaseManager.AwaitDataArrayResponse(sqlRequest, out bool success);
            if (!success)
            {
                return;
            }
            string[] data = dataArrayResponse.Result;
            if (!dataArrayResponse.Success || data.Length != sqlRequest.ExpectedColumns)
            {
                ApiError.Throw(ApiErrorCode.InvalidUser, server, "No account is associated with this email address.");
                return;
            }
            string isOnline = data[0];
            string encryptedName = data[1];
            string userid = data[2];
            server.Account = new Account(null, false, data[3]);
            if (!isOnline.Equals("0"))
            {
                ApiError.Throw(ApiErrorCode.AlreadyOnline, server, "Already logged in from another device.");
                return;
            }
            AesContext aesContext = new AesContext(userid);
            string name = aesContext.DecryptOrDefault(encryptedName);
            server.Account = new Account
            {
                AuthenticationCode = SecurityManager.GenerateSecurityCode(),
                AuthenticationId = ApiRequestId.ConfirmPasswordReset,
                AuthenticationTime = DatabaseEssentials.GetTimeStamp()
            };
            EmailManager emailManager = EmailManager.Create(Subject.ResetPassword, Email, string.IsNullOrEmpty(name) ? "user" : name, server.Account.AuthenticationCode);
            emailManager.Send();
            GenericSuccessResponse response = new GenericSuccessResponse(ResponseId.PasswordReset, true);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(response);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}