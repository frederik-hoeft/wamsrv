using System;
using System.Collections.Generic;
using System.Text;
using wamsrv.ApiResponses;
using wamsrv.Database;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.ApiRequests
{
    public class GetAllEventsRequest : ApiRequest
    {
        public readonly bool IncludeExpired;
        public GetAllEventsRequest(ApiRequestId requestId, bool includeExpired)
        {
            RequestId = requestId;
            IncludeExpired = includeExpired;
        }

        public override void Process(ApiServer server)
        { 
            if (server.AssertServerSetup(this) || server.AssertUserOnline())
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager(server);
            string query = "SELECT hid FROM Tbl_event" + (IncludeExpired ? string.Empty : " WHERE expires > " + DatabaseEssentials.GetTimeStamp().ToString()) + ";";
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.Get2DArray, query, 1);
            Sql2DArrayResponse sql2DArrayResponse = databaseManager.Await2DArrayResponse(sqlRequest, out bool success);
            if (!success)
            {
                return;
            }
            GetAllEventsResponse response = new GetAllEventsResponse(ResponseId.GetAllEvents, sql2DArrayResponse.Result);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(response);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}
