namespace wamsrv.ApiResponses
{
    public class GetEventInfoResponse : ApiResponse
    {
        public readonly EventInfo EventInfo;
        public GetEventInfoResponse(ResponseId responseId, EventInfo eventInfo)
        {
            ResponseId = responseId;
            EventInfo = eventInfo;
        }
    }
}
