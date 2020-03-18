using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiResponses
{
    public class GetEventResponse : ApiResponse
    {
        public readonly Event Event;
        private GetEventResponse(ResponseId responseId, Event @event)
        {
            ResponseId = responseId;
            Event = @event;
        }

        public static GetEventResponse Create(ResponseId responseId, Event @event)
        {
            return new GetEventResponse(responseId, @event);
        }
    }
}
