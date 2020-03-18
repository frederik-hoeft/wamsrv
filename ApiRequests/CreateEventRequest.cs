using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiRequests
{
    public class CreateEventRequest : ApiRequest
    {
        public readonly EventInfo EventInfo;
        public CreateEventRequest(RequestId requestId, EventInfo eventInfo)
        {
            RequestId = requestId;
            EventInfo = eventInfo;
        }
        public override void Process(ApiClient client)
        {
            throw new NotImplementedException();
        }
    }
}
