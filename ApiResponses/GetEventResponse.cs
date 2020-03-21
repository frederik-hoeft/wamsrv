using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiResponses
{
    public class GetEventResponse : ApiResponse
    {
        public readonly Event Event;
        public GetEventResponse(ResponseId responseId, Event @event)
        {
            ResponseId = responseId;
            Event = @event;
        }
    }
}
