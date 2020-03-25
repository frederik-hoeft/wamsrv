namespace wamsrv.ApiResponses
{
    public class CreateCookieResponse : ApiResponse
    {
        public readonly string SecurityToken;
        public readonly Permission Permissions;
        public CreateCookieResponse(ResponseId responseId, string securityToken, Permission permissions)
        {
            ResponseId = responseId;
            SecurityToken = securityToken;
            Permissions = permissions;
        }
    }
}
