namespace wamsrv.ApiResponses
{
    public class GetEventResponseA : ApiResponse
    {
        public readonly Event Event;

        public GetEventResponseA(ResponseId responseId, Event @event)
        {
            ResponseId = responseId;
            Event = @event;
        }
    }
}