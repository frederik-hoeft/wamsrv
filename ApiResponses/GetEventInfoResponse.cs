using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiResponses
{
    public class GetEventInfoResponse : ApiResponse
    {
        public readonly EventInfo EventInfo;
        private GetEventInfoResponse(ResponseId responseId, EventInfo eventInfo)
        {
            ResponseId = responseId;
            EventInfo = eventInfo;
        }

        public static GetEventInfoResponse Create(ResponseId responseId, EventInfo eventInfo)
        {
            return new GetEventInfoResponse(responseId, eventInfo);
        }
    }
}
