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