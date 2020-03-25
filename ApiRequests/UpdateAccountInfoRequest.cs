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
        public readonly string Name;
        public readonly string Occupation;
        public readonly string Info1;
        public readonly string Info2;
        public readonly string Info3;
        public readonly string Info4;
        public readonly string Info5;
        public readonly string Info6;
        public readonly string Info7;
        public readonly string Info8;
        public readonly string Info9;
        public readonly string Info10;
        public readonly string Location;
        public readonly int Radius;
        public readonly bool IsVisible;
        public readonly bool ShowLog;
        public UpdateAccountInfoRequest(ApiRequestId requestId, string name, string occupation, string info1, string info2, string info3, string info4, string info5, string info6, string info7, string info8, string info9, string info10, string location, int radius, bool isVisible, bool showLog)
        {
            RequestId = requestId;
            Name = name;
            Occupation = occupation;
            Info1 = info1;
            Info2 = info2;
            Info3 = info3;
            Info4 = info4;
            Info5 = info5;
            Info6 = info6;
            Info7 = info7;
            Info8 = info8;
            Info9 = info9;
            Info10 = info10;
            Location = location;
            Radius = radius;
            IsVisible = isVisible;
            ShowLog = showLog;
        }
        public override void Process(ApiServer server)
        {
            server.RequestId = RequestId;
            if (server.AssertUserOnline() || server.AssertIdSet() ||server.AssertAccountInfoNotNull())
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
            string cryptoName = aesContext.EncryptOrDefault(Name);
            string cryptoOccupation = aesContext.EncryptOrDefault(Occupation);
            StringBuilder stringBuilder = new StringBuilder();
            string[] infos = new string[] { Info1, Info2, Info3, Info4, Info5, Info6, Info7, Info8, Info9, Info10 };
            for (int i = 0; i < infos.Length; i++)
            {
                stringBuilder.Append(", info" + (i + 1).ToString() + " = \'" + aesContext.EncryptOrDefault(infos[i]) + "\'");
            }
            query = "UPDATE Tbl_user SET name = \'" + cryptoName + "\', occupation = \'" + cryptoOccupation + "\'" + stringBuilder.ToString() + ", location = \'" + DatabaseEssentials.Security.Sanitize(Location) + "\', radius = " + Radius.ToString() + ", isVisible = " + (IsVisible ? "1" : "0") + ", showLog = " +( ShowLog ? "1" : "0") + " WHERE id = " + DatabaseEssentials.Security.Sanitize(server.Account.Id) + ";";
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
