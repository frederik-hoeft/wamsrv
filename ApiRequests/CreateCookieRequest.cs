﻿using System;
using wamsrv.ApiResponses;
using wamsrv.Database;
using wamsrv.Security;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.ApiRequests
{
    public class CreateCookieRequest : ApiRequest
    {
        public readonly string Email;
        public readonly string Password;
        public readonly string Info;
        public CreateCookieRequest(ApiRequestId requestId, string email, string password, string info)
        {
            RequestId = requestId;
            Email = email;
            Password = password;
            Info = info;
        }
        public override void Process(ApiServer server)
        {
            if (server.AssertAccountNotNull(RequestId))
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager();
            string query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "SELECT id, password, isOnline FROM Tbl_user WHERE email = \"", Email, "\" LIMIT 1;" });
            SqlApiRequest apiRequest = SqlApiRequest.Create(SqlRequestId.GetDataArray, query, 3);
            SqlDataArrayResponse dataArrayResponse = databaseManager.AwaitDataArrayResponse(apiRequest);
            string[] data = dataArrayResponse.Result;
            if (!dataArrayResponse.Success || data.Length != 3)
            {
                string errorCode = ApiError.Throw(ApiErrorCode.UnknownUser, RequestId, "No account is associated with this email address.");
                server.Network.Send(errorCode);
                return;
            }
            string id = data[0];
            string hash = data[1];
            if (Convert.ToInt32(data[2]) == 1)
            {
                string errorCode = ApiError.Throw(ApiErrorCode.AlreadyOnline, RequestId, "Already logged in from another device.");
                server.Network.Send(errorCode);
                return;
            }
            bool authenticationSuccessful = SecurityManager.ScryptCheck(Password, hash);
            if (!authenticationSuccessful)
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InvalidCredentials, RequestId, "Incorrect password.");
                server.Network.Send(errorCode);
                return;
            }
            bool success = databaseManager.SetUserOnline(id);
            if (!success)
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InternalServerError, RequestId, "Unable to change online status.");
                server.Network.Send(errorCode);
                return;
            }
            string securityToken = SecurityManager.GenerateSecurityToken();
            // Token should expire every month.
            int expirationDate = DatabaseEssentials.GetTimeStamp() + MainServer.Config.WamsrvSecurityConfig.SecurityTokenExpirationTime;
            query = DatabaseEssentials.Security.SanitizeQuery(new string[] { "INSERT INTO Tbl_cookies (userid, value, expires, info) VALUES (", id, ",\"", securityToken, "\",", expirationDate.ToString(), ",\"", Info, "\");" });
            apiRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
            SqlModifyDataResponse modifyDataResponse = databaseManager.AwaitModifyDataResponse(apiRequest);
            if (!modifyDataResponse.Success)
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InternalServerError, RequestId, "Unable to generate security token.");
                server.Network.Send(errorCode);
                return;
            }
            if (server.Account == null)
            {
                server.Account = databaseManager.GetAccount(id, out success);
                if (!success)
                {
                    string errorCode = ApiError.Throw(ApiErrorCode.InternalServerError, RequestId, "Unable to fetch account info.");
                    return;
                }
            }
            server.Account.IsOnline = true;
            CreateCookieResponse apiResponse = new CreateCookieResponse(ResponseId.CreateCookie, securityToken);
            ApiResponses.SerializedApiResponse serializedApiResponse = ApiResponses.SerializedApiResponse.Create(apiResponse);
            string json = serializedApiResponse.Serialize();
            server.Network.Send(json);
        }
    }
}
