using System;
using wamsrv.ApiResponses;
using wamsrv.Database;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.ApiRequests
{
    public class EditEventRequestA : ApiRequest
    {
        public readonly EventInfo EventInfo;
        public EditEventRequestA(ApiRequestId requestId, EventInfo eventInfo)
        {
            RequestId = requestId;
            EventInfo = eventInfo;
        }
        public override void Process(ApiServer server)
        {
            if (server.AssertServerSetup(this) || server.AssertIdSet() || server.AssertUserOnline() || server.AssertHasPermission(Permission.CREATE_EVENT) || server.AssertEventInfoNotNull(EventInfo))
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager(server);
            if (!databaseManager.CheckEventExists(EventInfo.EventId))
            {
                return;
            }
            string query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "UPDATE Tbl_event SET userid = ", server.Account.Id, ", title = \'", EventInfo.Title, "\', expires = ", EventInfo.ExpirationDate.ToString(), ", date = \'", EventInfo.Date, "\', time = \'", EventInfo.Time, "\', location = \'", EventInfo.Location, "\', url = \'", EventInfo.Url, "\', image = \'", EventInfo.Image, "\', description = \'", EventInfo.Description, "\'  WHERE hid = \'", EventInfo.EventId, "\';" });
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = databaseManager.AwaitModifyDataResponse(sqlRequest, out bool success);
            if (!success)
            {
                return;
            }
            if (!modifyDataResponse.Success)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to update event.");
                return;
            }
            GenericSuccessResponse response = new GenericSuccessResponse(ResponseId.EditEventA, true);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(response);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}
