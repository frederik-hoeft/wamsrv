using System;

namespace wamsrv.ApiRequests
{
    public class GetEventInfoRequest : ApiRequest
    {
        public readonly int EventId;
        public GetEventInfoRequest(ApiRequestId requestId, int eventId)
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
