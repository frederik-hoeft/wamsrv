using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiResponses
{
    public class CreateEventResponseA: ApiResponse
    {
        public readonly string EventId;
        public CreateEventResponseA(ResponseId responseId, string eventId)
        {
            ResponseId = responseId;
            EventId = eventId;
        }
    }
}
