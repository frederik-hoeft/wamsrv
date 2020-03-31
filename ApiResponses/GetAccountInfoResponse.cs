namespace wamsrv.ApiResponses
{
    public class GetAccountInfoResponse : ApiResponse
    {
        public readonly AccountInfo AccountInfo;

        public GetAccountInfoResponse(ResponseId responseId, AccountInfo accountInfo)
        {
            ResponseId = responseId;
            AccountInfo = accountInfo;
        }
    }
}