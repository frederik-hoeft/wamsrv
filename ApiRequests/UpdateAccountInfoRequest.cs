using System.Text;
using wamsrv.ApiResponses;
using wamsrv.Database;
using wamsrv.Security;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.ApiRequests
{
    public class UpdateAccountInfoRequest : ApiRequest
    {
        public readonly AccountInfo AccountInfo;
        public UpdateAccountInfoRequest(ApiRequestId requestId, AccountInfo accountInfo)
        {
            RequestId = requestId;
            AccountInfo = accountInfo;
        }
        public override void Process(ApiServer server)
        {
            if (server == null)
            {
                return;
            }
            server.RequestId = RequestId;
            if (AccountInfo == null)
            {
                ApiError.Throw(ApiErrorCode.InvalidArgument, server, "AccountInfo was null.");
                return;
            }
            if (server.AssertUserOnline() || server.AssertIdSet() || server.AssertAccountInfoNotNull())
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager(server);
            string query;
            bool success;
            if (string.IsNullOrEmpty(server.Account.AccountInfo.UserId))
            {
                query = "SELECT hid FROM Tbl_user WHERE id = " + DatabaseEssentials.Security.Sanitize(server.Account.Id);
                SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
                SqlSingleOrDefaultResponse singleOrDefaultResponse = databaseManager.AwaitSingleOrDefaultResponse(sqlRequest, out success);
                if (!success)
                {
                    return;
                }
                if (!singleOrDefaultResponse.Success)
                {
                    ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to determine userid.");
                    return;
                }
                server.Account.AccountInfo.UserId = singleOrDefaultResponse.Result;
            }
            AesContext aesContext = new AesContext(server.Account.AccountInfo.UserId);
            string cryptoName = aesContext.EncryptOrDefault(AccountInfo.Name);
            string cryptoOccupation = aesContext.EncryptOrDefault(AccountInfo.Occupation);
            StringBuilder stringBuilder = new StringBuilder();
            string[] infos = new string[] { AccountInfo.Info1, AccountInfo.Info2, AccountInfo.Info3, AccountInfo.Info4, AccountInfo.Info5, AccountInfo.Info6, AccountInfo.Info7, AccountInfo.Info8, AccountInfo.Info9, AccountInfo.Info10 };
            for (int i = 0; i < infos.Length; i++)
            {
                stringBuilder.Append(", info" + (i + 1).ToString() + " = \'" + aesContext.EncryptOrDefault(infos[i]) + "\'");
            }
            query = "UPDATE Tbl_user SET name = \'" + cryptoName + "\', occupation = \'" + cryptoOccupation + "\'" + stringBuilder.ToString() + ", location = \'" + DatabaseEssentials.Security.Sanitize(AccountInfo.Location) + "\', radius = " + AccountInfo.Radius.ToString() + ", isVisible = " + (AccountInfo.IsVisible ? "1" : "0") + ", showLog = " +(AccountInfo.ShowLog ? "1" : "0") + " WHERE id = " + DatabaseEssentials.Security.Sanitize(server.Account.Id) + ";";
            SqlApiRequest sqlApiRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = databaseManager.AwaitModifyDataResponse(sqlApiRequest, out success);
            if (!success)
            {
                return;
            }
            if (!modifyDataResponse.Success)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to update account info.");
                return;
            }
            GenericSuccessResponse successResponse = new GenericSuccessResponse(ResponseId.UpdateAccountInfo, true);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(successResponse);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
        }
    }
}
