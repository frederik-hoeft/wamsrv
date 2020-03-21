using System;

namespace wamsrv.ApiRequests
{
    public class GetEventRequest : ApiRequest
    {
        public readonly int EventId;
        public GetEventRequest(ApiRequestId requestId, int eventId)
        {
            RequestId = requestId;
            EventId = eventId;
        }
        public override void Process(ApiServer server)
        {
            throw new NotImplementedException();
        }
    }
}
