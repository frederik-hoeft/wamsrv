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
            server.RequestId = RequestId;
            if (server.AssertAccountNotNull() || server.AssertUserOnline())
            {
                server.UnitTesting.MethodSuccess = false;
                return;
            }
            server.UnitTesting.RequestId = RequestId;
            using DatabaseManager databaseManager = new DatabaseManager(server);
            string id = databaseManager.UserIdToId(UserId, out SqlErrorState sqlErrorState);
            if (sqlErrorState != SqlErrorState.Success)
            {
                if (sqlErrorState == SqlErrorState.GenericError)
                {
                    ApiError.Throw(ApiErrorCode.InvalidUser, server, "User could not be found.");
                }
                return;
            }
            Account account = databaseManager.GetAccount(id, out sqlErrorState);
            if (sqlErrorState != SqlErrorState.Success)
            {
                if (sqlErrorState == SqlErrorState.GenericError)
                {
                    ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to fetch account info.");
                }
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
