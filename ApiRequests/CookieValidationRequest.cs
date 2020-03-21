﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using wamsrv.ApiResponses;
using wamsrv.Database;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.ApiRequests
{
    public class CookieValidationRequest : ApiRequest
    {
        public readonly string Value;
        public CookieValidationRequest(ApiRequestId requestId, string value)
        {
            RequestId = requestId;
            Value = value;
        }
        public override void Process(ApiServer server)
        {
            using DatabaseManager databaseManager = new DatabaseManager();
            string query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "SELECT userid FROM Tbl_cookies WHERE value = \"", Value, "\" AND expires > ", DatabaseEssentials.GetTimeStamp().ToString(), ";" });
            SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.GetSingleOrDefault, query, 1);
            SqlSingleOrDefaultResponse singleOrDefaultResponse = databaseManager.GetSingleOrDefaultResponse(sqlRequest);
            if (!singleOrDefaultResponse.Success)
            {
                // TODO: return error message
                return;
            }
            bool parseSuccess = int.TryParse(singleOrDefaultResponse.Result, out int id);
            if (!parseSuccess)
            {
                // TODO: return error code
                return;
            }
            // TODO: refactor --> duplicate code in CreateCookieRequest.cs
            query = "UPDATE Tbl_data SET isOnline = 1 WHERE id = " + id.ToString();
            sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = databaseManager.GetModifyDataResponse(sqlRequest);
            if (!modifyDataResponse.Success)
            {
                // TODO: return error message
                return;
            }
            if (server.Account == null)
            {
                query = "SELECT hid, name, occupation, info, location, email, radius, isVisible, showLog FROM Tbl_user WHERE id = " + id.ToString() + " LIMIT 1;";
                sqlRequest = SqlApiRequest.Create(SqlRequestId.GetDataArray, query, 9);
                SqlDataArrayResponse dataArrayResponse = databaseManager.GetDataArrayResponse(sqlRequest);
                string[] account = dataArrayResponse.Result;
                if (!dataArrayResponse.Success || account.Length != 9)
                {
                    // TODO: return error message
                    return;
                }
                string userid = account[0];
                string userSecret = SecurityManager.DeriveUserSecret(userid);
                string name = SecurityManager.AESDecrypt(account[1], userSecret);
                string occupation = SecurityManager.AESDecrypt(account[2], userSecret);
                string info = SecurityManager.AESDecrypt(account[3], userSecret);
                string location = SecurityManager.AESDecrypt(account[4], userSecret);
                string email = account[5];
                bool successParse1 = int.TryParse(account[6], out int radius);
                bool successParse2 = int.TryParse(account[7], out int isVisible);
                bool successParse3 = int.TryParse(account[8], out int showLog);
                if (!successParse1 || !successParse2 || !successParse3)
                {
                    // TODO: return error message
                    return;
                }
                AccountInfo accountInfo = new AccountInfo(name, occupation, info, location, radius, userid, email, Convert.ToBoolean(isVisible), Convert.ToBoolean(showLog));
                server.Account = new Account(accountInfo, false);
            }
            CookieValidationResponse apiResponse = new CookieValidationResponse(ResponseId.CreateCookie, true);
            ApiResponses.SerializedApiResponse serializedApiResponse = ApiResponses.SerializedApiResponse.Create(apiResponse);
            string json = serializedApiResponse.Serialize();
            server.Network.Send(json);
        }
    }
}
