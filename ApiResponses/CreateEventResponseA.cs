namespace wamsrv.ApiResponses
{
    public class CreateEventResponseA : ApiResponse
    {
        public readonly string EventId;

        public CreateEventResponseA(ResponseId responseId, string eventId)
        {
            ResponseId = responseId;
            EventId = eventId;
        }
    }
}