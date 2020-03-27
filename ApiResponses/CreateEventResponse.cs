using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiResponses
{
    public class CreateEventResponse: ApiResponse
    {
        public readonly string EventId;
        public CreateEventResponse(ResponseId responseId, string eventId)
        {
            ResponseId = responseId;
            EventId = eventId;
        }
    }
}
