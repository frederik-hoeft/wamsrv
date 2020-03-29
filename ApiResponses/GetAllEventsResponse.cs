using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiResponses
{
    public class GetAllEventsResponse : ApiResponse
    {
        public readonly string[][] EventIds;
        public GetAllEventsResponse(ResponseId responseId, string[][] eventIds)
        {
            ResponseId = responseId;
            EventIds = eventIds;
        }
    }
}
