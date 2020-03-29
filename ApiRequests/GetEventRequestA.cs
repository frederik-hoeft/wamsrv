using System;
using wamsrv.ApiResponses;
using wamsrv.Database;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.ApiRequests
{
    public class GetEventRequestA : ApiRequest
    {
        public readonly string EventId;
        public GetEventRequestA(ApiRequestId requestId, string eventId)
        {
            RequestId = requestId;
            EventId = eventId;
        }
        public override void Process(ApiServer server)
        {
            if (server.AssertServerSetup(this) || server.AssertUserOnline() || server.AssertHasPermission(Permission.QUERY_EVENT_INFO))
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
            string query = "SELECT u.hid FROM Tbl_event as e, Tbl_user as u WHERE e.hid = \'" + DatabaseEssentials.Security.Sanitize(EventId) + "\' AND e.userid = u.id LIMIT 1;";
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
            SqlSingleOrDefaultResponse singleOrDefaultResponse = databaseManager.AwaitSingleOrDefaultResponse(sqlRequest, out success);
            if (!success)
            {
                return;
            }
            if (!singleOrDefaultResponse.Success)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to fetch event.");
                return;
            }
            GetEventResponseA response = new GetEventResponseA(ResponseId.GetEventA, new Event(eventInfo, singleOrDefaultResponse.Result));
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(response);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}
