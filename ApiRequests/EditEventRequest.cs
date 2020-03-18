using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiRequests
{
    public class EditEventRequest : ApiRequest
    {
        public readonly int EventId;
        public readonly EventInfo EventInfo;
        public EditEventRequest(ApiRequestId requestId, int eventId, EventInfo eventInfo)
        {
            RequestId = requestId;
            EventId = eventId;
            EventInfo = eventInfo;
        }
        public override void Process(ApiServer server)
        {
            throw new NotImplementedException();
        }
    }
}
