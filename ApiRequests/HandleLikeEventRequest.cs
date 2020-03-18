using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiRequests
{
    class HandleLikeEventRequest : ApiRequest
    {
        public readonly string UserId;
        public HandleLikeEventRequest(ApiRequestId requestId, string userid)
        {
            RequestId = requestId;
            UserId = userid;
        }
        public override void Process(ApiServer server)
        {
            throw new NotImplementedException();
        }
    }
}
