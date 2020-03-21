namespace wamsrv.ApiResponses
{
    public class CreateCookieResponse : ApiResponse
    {
        public readonly string SecurityToken;
        public CreateCookieResponse(ResponseId responseId, string securityToken)
        {
            ResponseId = responseId;
            SecurityToken = securityToken;
        }
    }
}
