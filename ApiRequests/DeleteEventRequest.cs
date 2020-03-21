using System;

namespace wamsrv.ApiRequests
{
    public class DeleteEventRequest : ApiRequest
    {
        public readonly int EventId;
        public DeleteEventRequest(ApiRequestId requestId, int eventId)
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
