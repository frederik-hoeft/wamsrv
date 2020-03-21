using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiResponses
{
    public class CookieValidationResponse : ApiResponse
    {
        public readonly bool AuthenticationSuccessful;
        public CookieValidationResponse(ResponseId responseId, bool authenticationSuccessful)
        {
            ResponseId = responseId;
            AuthenticationSuccessful = authenticationSuccessful;
        }
    }
}
