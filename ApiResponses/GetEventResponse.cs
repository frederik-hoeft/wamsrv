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
