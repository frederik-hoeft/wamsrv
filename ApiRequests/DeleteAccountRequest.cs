using wamsrv.ApiResponses;
using wamsrv.Database;
using washared.DatabaseServer;

namespace wamsrv.ApiRequests
{
    public class DeleteAccountRequest : ApiRequest
    {
        public DeleteAccountRequest(ApiRequestId requestId)
        {
            RequestId = requestId;
        }

        public override void Process(ApiServer server)
        {
            if (server.AssertServerSetup(this) || server.AssertUserOnline() || server.AssertIdSet())
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager(server);
            if (databaseManager.OptionalAssertUserExists(server.Account.Id, true))
            {
                return;
            }
            string sanitizedId = DatabaseEssentials.Security.Sanitize(server.Account.Id);
            string deleteCookies = "DELETE FROM Tbl_cookies WHERE userid = " + sanitizedId + ";";
            string deleteAdmin = "DELETE FROM Tbl_admin WHERE userid = " + sanitizedId + ";";
            string deleteEvent = "DELETE FROM Tbl_event WHERE userid = " + sanitizedId + ";";
            string deleteLog = "DELETE FROM Tbl_log WHERE userid = " + sanitizedId + ";";
            string deleteLikes = "DELETE FROM Tbl_likes WHERE sourceid = " + sanitizedId + " OR targetid = " + sanitizedId + ";";
            string deleteDislikes = "DELETE FROM Tbl_dislikes WHERE sourceid = " + sanitizedId + " OR targetid = " + sanitizedId + ";";
            string deleteMatches = "DELETE FROM Tbl_match WHERE userid1 = " + sanitizedId + " OR userid2 = " + sanitizedId + ";";
            string query = deleteCookies + deleteAdmin + deleteEvent + deleteLog + deleteLikes + deleteDislikes + deleteMatches;
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            _ = databaseManager.AwaitModifyDataResponse(sqlRequest, out bool success);
            if (!success)
            {
                return;
            }
            GenericSuccessResponse response = new GenericSuccessResponse(ResponseId.DeleteAccount, true);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(response);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}