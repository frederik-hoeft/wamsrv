using System;
using wamsrv.Security;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.Database
{
    public partial class DatabaseManager
    {
        public bool CheckEmailAvailable(string email, out bool success)
        {
            string sanitizedEmail = DatabaseEssentials.Security.Sanitize(email);
            string query = "SELECT 1 FROM Tbl_user WHERE email = \'" + sanitizedEmail + "\' LIMIT 1;";
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
            SqlSingleOrDefaultResponse singleOrDefaultResponse = this.AwaitSingleOrDefaultResponse(sqlRequest, out bool sqlSuccess);
            success = sqlSuccess;
            return success && !singleOrDefaultResponse.Success;
        }
        public bool SetUserOnline(string id)
        {
            string query = "UPDATE Tbl_user SET isOnline = 1 WHERE id = " + id;
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = this.AwaitModifyDataResponse(sqlRequest, out bool success);
            return success && modifyDataResponse.Success;
        }

        public bool SetUserOffline(string id)
        {
            string query = "UPDATE Tbl_user SET isOnline = 0 WHERE id = " + id;
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = this.AwaitModifyDataResponse(sqlRequest, out bool success);
            return success && modifyDataResponse.Success;
        }

        public string UserIdToId(string userid, out SqlErrorState errorState)
        {
            string sanitizedUserId = DatabaseEssentials.Security.Sanitize(userid);
            string query = "SELECT id FROM Tbl_user WHERE hid = \'" + sanitizedUserId + "\' LIMIT 1;";
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
            SqlSingleOrDefaultResponse singleOrDefaultResponse = this.AwaitSingleOrDefaultResponse(sqlRequest, out bool success);
            if (!success)
            {
                errorState = SqlErrorState.SqlError;
                return string.Empty;
            }
            string id = singleOrDefaultResponse.Result;
            if (!singleOrDefaultResponse.Success)
            {
                errorState = SqlErrorState.GenericError;
                return string.Empty;
            }
            errorState = SqlErrorState.Success;
            return id;
        }
        /// <summary>
        /// Fetches the specified account from the database
        /// </summary>
        /// <param name="id">The id of the account</param>
        /// <param name="error">1 == need to print error message, 0 == success, -1 failed no error message</param>
        /// <returns></returns>
        public Account GetAccount(string id, out SqlErrorState errorState)
        {
            string query = "SELECT hid, name, occupation, info, location, email, radius, isVisible, showLog FROM Tbl_user WHERE id = " + id + " LIMIT 1;";
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetDataArray, query, 9);
            SqlDataArrayResponse dataArrayResponse = this.AwaitDataArrayResponse(sqlRequest, out bool success);
            if (!success)
            {
                errorState = SqlErrorState.SqlError;
                return null;
            }
            string[] account = dataArrayResponse.Result;
            if (!dataArrayResponse.Success || account.Length != 9)
            {
                errorState = SqlErrorState.GenericError;
                return null;
            }
            string userid = account[0];
            AesContext aesContext = new AesContext(userid);
            string name = aesContext.DecryptOrDefault(account[1]);
            string occupation = aesContext.DecryptOrDefault(account[2]);
            string info = aesContext.DecryptOrDefault(account[3]);
            string location = account[4];
            string email = account[5];
            bool successParse1 = int.TryParse(account[6], out int radius);
            bool successParse2 = int.TryParse(account[7], out int isVisible);
            bool successParse3 = int.TryParse(account[8], out int showLog);
            if (!successParse1 || !successParse2 || !successParse3)
            {
                errorState = SqlErrorState.GenericError;
                return null;
            }
            AccountInfo accountInfo = new AccountInfo(name, occupation, info, location, radius, userid, email, Convert.ToBoolean(isVisible), Convert.ToBoolean(showLog));
            errorState = SqlErrorState.Success;
            return new Account(accountInfo, false, id);
        }
    }
}
