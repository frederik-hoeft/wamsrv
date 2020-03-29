using System;
using System.Collections.Generic;
using System.Text;
using wamsrv.ApiResponses;
using wamsrv.Database;

namespace wamsrv.ApiRequests
{
    public class GetAccountInfoRequest : ApiRequest
    {
        public readonly string UserId;
        public GetAccountInfoRequest(ApiRequestId requestId, string userid)
        {
            RequestId = requestId;
            UserId = userid;
        }
        public override void Process(ApiServer server)
        {
            if (server.AssertServerSetup(this) || server.AssertAccountNotNull() || server.AssertUserOnline())
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager(server);
            string id = databaseManager.UserIdToId(UserId, out bool success);
            if (!success)
            {
                return;
            }
            Account account = databaseManager.GetAccount(id, out success);
            if (!success)
            {
                return;
            }
            account.AccountInfo.Radius = -1;
            GetAccountInfoResponse response = new GetAccountInfoResponse(ResponseId.GetAccountInfo, account.AccountInfo);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(response);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}
