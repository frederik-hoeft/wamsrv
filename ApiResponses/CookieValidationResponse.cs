namespace wamsrv.ApiResponses
{
    public class CookieValidationResponse : ApiResponse
    {
        public readonly bool AuthenticationSuccessful;
        public readonly Permission Permissions;
        public CookieValidationResponse(ResponseId responseId, bool authenticationSuccessful, Permission permissions)
        {
            ResponseId = responseId;
            AuthenticationSuccessful = authenticationSuccessful;
            Permissions = permissions;
        }
    }
}
