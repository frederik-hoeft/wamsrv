using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiRequests
{
    public class GetEventInfoRequest : ApiRequest
    {
        public readonly int EventId;
        public GetEventInfoRequest(RequestId requestId, int eventId)
        {
            RequestId = requestId;
            EventId = eventId;
        }

        public override void Process(ApiClient client)
        {
            throw new NotImplementedException();
        }
    }
}
