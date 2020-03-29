using System;
using wamsrv.ApiResponses;
using wamsrv.Database;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.ApiRequests
{
    public class DeleteEventRequestA : ApiRequest
    {
        public readonly string EventId;
        public DeleteEventRequestA(ApiRequestId requestId, string eventId)
        {
            RequestId = requestId;
            EventId = eventId;
        }
        public override void Process(ApiServer server)
        {
            if (server.AssertServerSetup(this) || server.AssertIdSet() || server.AssertUserOnline() || server.AssertHasPermission(Permission.CREATE_EVENT))
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager(server);
            if (!databaseManager.CheckEventExists(EventId))
            {
                return;
            }
            string query = "DELETE FROM Tbl_event WHERE hid = \'" + DatabaseEssentials.Security.Sanitize(EventId) + "\';";
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = databaseManager.AwaitModifyDataResponse(sqlRequest, out bool success);
            if (!success)
            {
                return;
            }
            if (!modifyDataResponse.Success)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to delete the requested event.");
                return;
            }
            GenericSuccessResponse response = new GenericSuccessResponse(ResponseId.DeleteEventA, true);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(response);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}
