using System;
using System.Text;
using wamsrv.ApiResponses;
using wamsrv.Security;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.Database
{
    public partial class DatabaseManager
    {
        public bool SetupAccount(string id)
        {
            if (server.Account == null)
            {
                server.Account = GetAccount(id, out bool success);
                if (!success)
                {
                    return false;
                }
            }
            return SetUserOnline();
        }

        public bool CheckEmailAvailable(string email, out bool success)
        {
            string sanitizedEmail = DatabaseEssentials.Security.Sanitize(email);
            string query = "SELECT 1 FROM Tbl_user WHERE email = \'" + sanitizedEmail + "\' LIMIT 1;";
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
            SqlSingleOrDefaultResponse singleOrDefaultResponse = this.AwaitSingleOrDefaultResponse(sqlRequest, out bool sqlSuccess);
            success = sqlSuccess;
            return success && !singleOrDefaultResponse.Success;
        }

        public bool SetUserOnline()
        {
            if (server.AssertIdSet())
            {
                return false;
            }
            string query = "UPDATE Tbl_user SET isOnline = 1 WHERE id = " + server.Account.Id;
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = AwaitModifyDataResponse(sqlRequest, out bool success);
            if (success && modifyDataResponse.Success)
            {
                server.Account.IsOnline = true;
            }
            return success && modifyDataResponse.Success;
        }

        public bool SetUserOffline()
        {
            if (server.AssertIdSet())
            {
                return false;
            }
            string query = "UPDATE Tbl_user SET isOnline = 0 WHERE id = " + server.Account.Id;
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = AwaitModifyDataResponse(sqlRequest, out bool success);
            if (success && modifyDataResponse.Success)
            {
                server.Account.IsOnline = false;
            }
            return success && modifyDataResponse.Success;
        }

        public bool CheckUserExists(string userId, out bool success)
        {
            string query = "SELECT 1 FROM Tbl_user WHERE hid = \'" + DatabaseEssentials.Security.Sanitize(userId) + "\' LIMIT 1;";
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
            SqlSingleOrDefaultResponse singleOrDefaultResponse = AwaitSingleOrDefaultResponse(sqlRequest, out bool sqlSuccess);
            if (!sqlSuccess)
            {
                success = false;
                return false;
            }
            success = true;
            return singleOrDefaultResponse.Success;
        }

        public Permission GetUserPermission(string userId, out bool success)
        {
            string id = UserIdToId(userId, out bool sqlSuccess);
            if (!sqlSuccess)
            {
                success = false;
                return Permission.NONE;
            }
            string query = "SELECT permissions FROM Tbl_admin WHERE userid = " + id + " LIMIT 1;";
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
            SqlSingleOrDefaultResponse singleOrDefaultResponse = AwaitSingleOrDefaultResponse(sqlRequest, out sqlSuccess);
            if (!sqlSuccess)
            {
                success = false;
                return Permission.NONE;
            }
            if (!singleOrDefaultResponse.Success)
            {
                success = true;
                return Permission.NONE;
            }
            bool parseSuccess = int.TryParse(singleOrDefaultResponse.Result, out int intPermission);
            if (!parseSuccess)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to parse user permission.");
                success = false;
                return Permission.NONE;
            }
            Permission permission = (Permission)intPermission;
            if (permission == Permission.NONE)
            {
                query = "DELETE FROM Tbl_admin WHERE userid = " + id + ";";
                sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
                AwaitModifyDataResponse(sqlRequest, out sqlSuccess);
                if (!sqlSuccess)
                {
                    success = false;
                    return Permission.NONE;
                }
            }
            success = true;
            return permission;
        }

        public bool UserIsRoot(string userId, out bool success)
        {
            string query = "SELECT email FROM Tbl_user WHERE hid = \'" + DatabaseEssentials.Security.Sanitize(userId) + "\' LIMIT 1;";
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
            SqlSingleOrDefaultResponse singleOrDefaultResponse = AwaitSingleOrDefaultResponse(sqlRequest, out bool sqlSuccess);
            if (!sqlSuccess)
            {
                success = false;
                return false;
            }
            if (!singleOrDefaultResponse.Success)
            {
                ApiError.Throw(ApiErrorCode.InvalidUser, server, "User could not be found.");
                success = false;
                return false;
            }
            success = true;
            return singleOrDefaultResponse.Result.Equals(MainServer.Config.WamsrvEmailConfig.EmailAddress);
        }

        public bool OptionalAssertUserExists(string id, bool isDatabaseId)
        {
            if (MainServer.Config.AdvancedErrorChecking)
            {
                string query;
                if (isDatabaseId)
                {
                    query = "SELECT 1 FROM Tbl_user WHERE id = " + DatabaseEssentials.Security.Sanitize(id) + ";";
                }
                else
                {
                    query = "SELECT 1 FROM Tbl_user WHERE hid = \'" + DatabaseEssentials.Security.Sanitize(id) + "\';";
                }
                SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
                SqlSingleOrDefaultResponse singleOrDefaultResponse = AwaitSingleOrDefaultResponse(sqlRequest, out bool success);
                if (!success)
                {
                    return true;
                }
                return !singleOrDefaultResponse.Success;
            }
            return false;
        }

        /// <summary>
        /// Throws an exception if the user has insufficient permissions and returns false otherwise.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public bool AssertHasPermission(Permission permission)
        {
            if (server.AssertUserIdSet())
            {
                return true;
            }
            bool userExists = CheckUserExists(server.Account.AccountInfo.UserId, out bool success);
            if (!success)
            {
                return true;
            }
            if (!userExists)
            {
                ApiError.Throw(ApiErrorCode.InvalidUser, server, "User does not exist.");
                return true;
            }
            if (server.Account.AccountInfo.Email.Equals(MainServer.Config.WamsrvEmailConfig.EmailAddress))
            {
                return false;
            }
            Permission actualPermission = GetUserPermission(server.Account.AccountInfo.UserId, out success);
            if (!success)
            {
                return true;
            }
            if (actualPermission == Permission.NONE)
            {
                ApiError.Throw(ApiErrorCode.AccessDenied, server, "The requested action is not permitted in the current user context.");
                return true;
            }
            if (((int)actualPermission & (int)permission) != (int)permission)
            {
                ApiError.Throw(ApiErrorCode.InsufficientPermissions, server, "The requested action requires the following permission: " + permission.ToString());
                return true;
            }
            return false;
        }

        public string UserIdToId(string userid, out bool success)
        {
            string sanitizedUserId = DatabaseEssentials.Security.Sanitize(userid);
            string query = "SELECT id FROM Tbl_user WHERE hid = \'" + sanitizedUserId + "\' LIMIT 1;";
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
            SqlSingleOrDefaultResponse singleOrDefaultResponse = this.AwaitSingleOrDefaultResponse(sqlRequest, out bool sqlSuccess);
            if (!sqlSuccess)
            {
                success = false;
                return string.Empty;
            }
            string id = singleOrDefaultResponse.Result;
            if (!singleOrDefaultResponse.Success)
            {
                ApiError.Throw(ApiErrorCode.InvalidUser, server, "User could not be found.");
                success = false;
                return string.Empty;
            }
            success = true;
            return id;
        }

        /// <summary>
        /// Fetches the specified account from the database
        /// </summary>
        /// <param name="id">The id of the account</param>
        /// <returns></returns>
        public Account GetAccount(string id, out bool success)
        {
            StringBuilder infos = new StringBuilder();
            for (int i = 1; i < 11; i++)
            {
                infos.Append(", info").Append(i.ToString());
            }
            string query = "SELECT hid, name, occupation" + infos.ToString() + ", location, email, radius, isVisible, showLog FROM Tbl_user WHERE id = " + id + " LIMIT 1;";
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetDataArray, query, 18);
            SqlDataArrayResponse dataArrayResponse = AwaitDataArrayResponse(sqlRequest, out bool sqlSuccess);
            if (!sqlSuccess)
            {
                success = false;
                return null;
            }
            string[] account = dataArrayResponse.Result;
            if (!dataArrayResponse.Success || account.Length != 18)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to fetch account info.");
                success = false;
                return null;
            }
            string userid = account[0];
            AesContext aesContext = new AesContext(userid);
            string name = aesContext.DecryptOrDefault(account[1]);
            string occupation = aesContext.DecryptOrDefault(account[2]);
            string info1 = aesContext.DecryptOrDefault(account[3]);
            string info2 = aesContext.DecryptOrDefault(account[4]);
            string info3 = aesContext.DecryptOrDefault(account[5]);
            string info4 = aesContext.DecryptOrDefault(account[6]);
            string info5 = aesContext.DecryptOrDefault(account[7]);
            string info6 = aesContext.DecryptOrDefault(account[8]);
            string info7 = aesContext.DecryptOrDefault(account[9]);
            string info8 = aesContext.DecryptOrDefault(account[10]);
            string info9 = aesContext.DecryptOrDefault(account[11]);
            string info10 = aesContext.DecryptOrDefault(account[12]);
            string location = account[13];
            string email = account[14];
            bool successParse1 = int.TryParse(account[15], out int radius);
            bool successParse2 = int.TryParse(account[16], out int isVisible);
            bool successParse3 = int.TryParse(account[17], out int showLog);
            if (!successParse1 || !successParse2 || !successParse3)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to fetch account info.");
                success = false;
                return null;
            }
            AccountInfo accountInfo = new AccountInfo(name, occupation, info1, info2, info3, info4, info5, info6, info7, info8, info9, info10, location, radius, userid, email, Convert.ToBoolean(isVisible), Convert.ToBoolean(showLog));
            success = true;
            return new Account(accountInfo, false, id);
        }

        /// <summary>
        /// Updates the password of the current account. Requires server.Account.Id and server.Account.Password to be set.
        /// </summary>
        /// <returns></returns>
        public bool UpdatePassword()
        {
            string query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "UPDATE Tbl_user SET password = \'", server.Account.Password, "\' WHERE id = ", server.Account.Id, ";" });
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = AwaitModifyDataResponse(sqlRequest, out bool success);
            if (!success)
            {
                return true;
            }
            if (!modifyDataResponse.Success)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to update password.");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Throws an exception if the EventID is invalid and returns true otherwise.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public bool AssertEventExists(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                ApiError.Throw(ApiErrorCode.InvalidArgument, server, "Invalid argument: EventID was null.");
                return true;
            }
            string query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "SELECT 1 FROM Tbl_event WHERE hid = \'", eventId, "\' LIMIT 1;" });
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
            SqlSingleOrDefaultResponse singleOrDefaultResponse = AwaitSingleOrDefaultResponse(sqlRequest, out bool success);
            if (!success)
            {
                return true;
            }
            if (!singleOrDefaultResponse.Success)
            {
                ApiError.Throw(ApiErrorCode.NotFound, server, "There is no event associated with this EventID.");
                return true;
            }
            return false;
        }

        public EventInfo GetEventInfo(string eventId, out bool success)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                ApiError.Throw(ApiErrorCode.InvalidArgument, server, "Invalid argument: EventID was null.");
                success = false;
                return null;
            }
            string query = "SELECT title, expires, date, time, location, url, image, description FROM Tbl_event WHERE hid = \'" + DatabaseEssentials.Security.Sanitize(eventId) + "\';";
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetDataArray, query, 8);
            SqlDataArrayResponse dataArrayResponse = AwaitDataArrayResponse(sqlRequest, out bool sqlSuccess);
            if (!sqlSuccess)
            {
                success = false;
                return null;
            }
            if (!dataArrayResponse.Success || dataArrayResponse.Result.Length != 8)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to fetch EventInfo.");
                success = false;
                return null;
            }
            string[] data = dataArrayResponse.Result;
            if (!int.TryParse(data[1], out int expirationDate))
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to fetch EventInfo: failed to parse expiration date.");
                success = false;
                return null;
            }
            success = true;
            return new EventInfo(eventId, data[0], expirationDate, data[2], data[3], data[4], data[5], data[6], data[7]);
        }

        public bool DeleteSecurityTokens(string[] exceptions)
        {
            string exceptionsQueryExtension = string.Empty;
            if (exceptions.Length > 0)
            {
                StringBuilder stringBuilder = new StringBuilder(" AND value NOT IN (");
                for (int i = 0; i < exceptions.Length; i++)
                {
                    if (i == 0)
                    {
                        stringBuilder.Append("\'");
                    }
                    else
                    {
                        stringBuilder.Append(", \'");
                    }
                    stringBuilder.Append(DatabaseEssentials.Security.Sanitize(exceptions[i]));
                    stringBuilder.Append("\'");
                }
                stringBuilder.Append(")");
                exceptionsQueryExtension = stringBuilder.ToString();
            }
            string query = "DELETE FROM Tbl_cookies WHERE userid = " + DatabaseEssentials.Security.Sanitize(server.Account.Id) + exceptionsQueryExtension + ";";
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = AwaitModifyDataResponse(sqlRequest, out bool success);
            if (!success)
            {
                return true;
            }
            if (!modifyDataResponse.Success)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Unable to delete deprecated security tokens.");
                return true;
            }
            return false;
        }
    }
}