using System;
using System.Collections.Generic;
using System.Text;
using washared.DatabaseServer.ApiResponses;

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
