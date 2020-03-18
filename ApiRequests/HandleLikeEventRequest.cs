﻿using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiRequests
{
    class HandleLikeEventRequest : ApiRequest
    {
        public readonly string UserId;
        public HandleLikeEventRequest(RequestId requestId, string userid)
        {
            RequestId = requestId;
            UserId = userid;
        }
        public override void Process(ApiClient client)
        {
            throw new NotImplementedException();
        }
    }
}
