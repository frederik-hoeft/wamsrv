using System;
using wamsrv.ApiResponses;
using wamsrv.Database;
using wamsrv.Security;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.ApiRequests
{
    public class CreateEventRequestA : ApiRequest
    {
        public readonly EventInfo EventInfo;
        public CreateEventRequestA(ApiRequestId requestId, EventInfo eventInfo)
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
            string eventId = SecurityManager.GenerateHid();
            using DatabaseManager databaseManager = new DatabaseManager(server);
            string query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "INSERT INTO Tbl_event (userid, title, expires, date, time, location, url, image, description, hid) VALUES (", server.Account.Id, ", \'", EventInfo.Title, "\', ", EventInfo.ExpirationDate.ToString(), ", \'", EventInfo.Date, "\', \'", EventInfo.Time, "\', \'", EventInfo.Location, "\', \'", EventInfo.Url, "\', \'", EventInfo.Image, "\', \'", EventInfo.Description, "\', \'", eventId, "\');" });
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = databaseManager.AwaitModifyDataResponse(sqlRequest, out bool success);
            if (!success)
            {
                return;
            }
            if (!modifyDataResponse.Success)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to create event.");
                return;
            }
            CreateEventResponseA response = new CreateEventResponseA(ResponseId.CreateEvent, eventId);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(response);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}
