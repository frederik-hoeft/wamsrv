using System;
using wamsrv.ApiResponses;
using wamsrv.Database;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.ApiRequests
{
    public class EditEventRequest : ApiRequest
    {
        public readonly EventInfo EventInfo;
        public EditEventRequest(ApiRequestId requestId, EventInfo eventInfo)
        {
            RequestId = requestId;
            EventInfo = eventInfo;
        }
        public override void Process(ApiServer server)
        {
            if (server == null)
            {
                return;
            }
            server.RequestId = RequestId;
            if (EventInfo == null)
            {
                ApiError.Throw(ApiErrorCode.InvalidArgument, server, "Invalid argument: EventInfo was null.");
                return;
            }
            if (server.AssertAccountNotNull() || server.AssertIdSet() || server.AssertUserOnline() || server.AssertHasPermission(Permission.CREATE_EVENT))
            {
                return;
            }
            if (string.IsNullOrEmpty(EventInfo.EventId))
            {
                ApiError.Throw(ApiErrorCode.InvalidArgument, server, "Invalid argument: EventId was null.");
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager(server);
            string query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "SELECT 1 FROM Tbl_event WHERE hid = \'", EventInfo.EventId, "\' LIMIT 1;" });
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
            SqlSingleOrDefaultResponse singleOrDefaultResponse = databaseManager.AwaitSingleOrDefaultResponse(sqlRequest, out bool success);
            if (!success)
            {
                return;
            }
            if (!singleOrDefaultResponse.Success)
            {
                ApiError.Throw(ApiErrorCode.InvalidArgument, server, "Invalid argument: Event does not exist.");
                return;
            }
            query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "UPDATE Tbl_event SET userid = ", server.Account.Id, ", title = \'", EventInfo.Title, "\', expires = ", EventInfo.ExpirationDate.ToString(), ", date = \'", EventInfo.Date, "\', time = \'", EventInfo.Time, "\', location = \'", EventInfo.Location, "\', url = \'", EventInfo.Url, "\', image = \'", EventInfo.Image, "\', description = \'", EventInfo.Description, "\'  WHERE hid = \'", EventInfo.EventId, "\';" });
            sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = databaseManager.AwaitModifyDataResponse(sqlRequest, out success);
            if (!success)
            {
                return;
            }
            if (!modifyDataResponse.Success)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to update event.");
                return;
            }
            GenericSuccessResponse response = new GenericSuccessResponse(ResponseId.EditEvent, true);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(response);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}
