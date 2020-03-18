using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiRequests
{
    public class CreateEventRequest : ApiRequest
    {
        public readonly EventInfo EventInfo;
        public CreateEventRequest(ApiRequestId requestId, EventInfo eventInfo)
        {
            RequestId = requestId;
            EventInfo = eventInfo;
        }
        public override void Process(ApiServer server)
        {
            throw new NotImplementedException();
        }
    }
}
