using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiRequests
{
    public class HandleDislikeEventRequest : ApiRequest
    {
        public readonly string UserId;
        public HandleDislikeEventRequest(ApiRequestId requestId, string userid)
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
