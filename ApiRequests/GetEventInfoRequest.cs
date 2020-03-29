using System;
using wamsrv.ApiResponses;
using wamsrv.Database;

namespace wamsrv.ApiRequests
{
    public class GetEventInfoRequest : ApiRequest
    {
        public readonly string EventId;
        public GetEventInfoRequest(ApiRequestId requestId, string eventId)
        {
            RequestId = requestId;
            EventId = eventId;
        }

        public override void Process(ApiServer server)
        {
            if (server.AssertServerSetup(this) || server.AssertUserOnline())
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager(server);
            if (!databaseManager.CheckEventExists(EventId))
            {
                return;
            }
            EventInfo eventInfo = databaseManager.GetEventInfo(EventId, out bool success);
            if (!success)
            {
                return;
            }
            GetEventInfoResponse response = new GetEventInfoResponse(ResponseId.GetEventInfo, eventInfo);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(response);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}
