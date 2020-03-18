using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace wamsrv.ApiRequests
{
    public class CookieValidationRequest : ApiRequest
    {
        public readonly string Value;
        public CookieValidationRequest(RequestId requestId, string value)
        {
            RequestId = requestId;
            Value = value;
        }
        public override void Process(ApiClient client)
        {
            throw new NotImplementedException();
        }
    }
}
