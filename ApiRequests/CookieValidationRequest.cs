﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace wamsrv.ApiRequests
{
    public class CookieValidationRequest : ApiRequest
    {
        public readonly string Value;
        public CookieValidationRequest(ApiRequestId requestId, string value)
        {
            RequestId = requestId;
            Value = value;
        }
        public override void Process(ApiServer server)
        {
            throw new NotImplementedException();
        }
    }
}