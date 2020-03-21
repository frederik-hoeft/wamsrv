using System;
using wamsrv.Security;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.Database
{
    public partial class DatabaseManager
    {
        public bool SetUserOnline(string id)
        {
            string query = "UPDATE Tbl_data SET isOnline = 1 WHERE id = " + id;
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = AwaitModifyDataResponse(sqlRequest);
            return modifyDataResponse.Success;
        }

        public Account GetAccount(string id, out bool success)
        {
            string query = "SELECT hid, name, occupation, info, location, email, radius, isVisible, showLog FROM Tbl_user WHERE id = " + id + " LIMIT 1;";
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetDataArray, query, 9);
            SqlDataArrayResponse dataArrayResponse = AwaitDataArrayResponse(sqlRequest);
            string[] account = dataArrayResponse.Result;
            if (!dataArrayResponse.Success || account.Length != 9)
            {
                success = false;
                return null;
            }
            string userid = account[0];
            AesContext aesContext = new AesContext(userid);
            string name = aesContext.Decrypt(account[1]);
            string occupation = aesContext.Decrypt(account[2]);
            string info = aesContext.Decrypt(account[3]);
            string location = account[4];
            string email = account[5];
            bool successParse1 = int.TryParse(account[6], out int radius);
            bool successParse2 = int.TryParse(account[7], out int isVisible);
            bool successParse3 = int.TryParse(account[8], out int showLog);
            if (!successParse1 || !successParse2 || !successParse3)
            {
                success = false;
                return null;
            }
            AccountInfo accountInfo = new AccountInfo(name, occupation, info, location, radius, userid, email, Convert.ToBoolean(isVisible), Convert.ToBoolean(showLog));
            success = true;
            return new Account(accountInfo, false, id);
        }
    }
}
