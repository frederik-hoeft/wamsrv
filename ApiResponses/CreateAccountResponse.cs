namespace wamsrv.ApiResponses
{
    public class CreateAccountResponse : ApiResponse
    {
        public readonly bool EmailSendSuccess;
        public CreateAccountResponse(ResponseId responseId, bool emailSendSuccess)
        {
            ResponseId = responseId;
            EmailSendSuccess = emailSendSuccess;
        }
    }
}
