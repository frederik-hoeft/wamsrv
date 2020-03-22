using wamsrv.ApiRequests;
using wamsrv.ApiResponses;
using wamsrv.Database;

namespace wamsrv
{
    public static class ApiServerExtensions
    {
        /// <summary>
        /// Throws an exception if the account is not null and returns false otherwise.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static bool AssertAccountNull(this ApiServer server)
        {
            if (server.Account != null)
            {
                ApiError.Throw(ApiErrorCode.InvalidContext, server, "The requested action is invalid in this context.");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Throws an exception if the account is null and returns false otherwise.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static bool AssertAccountNotNull(this ApiServer server)
        {
            if (server.Account == null)
            {
                ApiError.Throw(ApiErrorCode.InvalidContext, server, "The requested action is invalid in this context.");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Throws an exception if the code is Invalid and returns false otherwise.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="requestId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool AssertAuthenticationCodeInvalid(this ApiServer server, string code)
        {
            if (server.AssertAccountNotNull())
            {
                return true;
            }
            if (string.IsNullOrEmpty(server.Account.AuthenticationCode))
            {
                ApiError.Throw(ApiErrorCode.InvalidContext, server, "The requested action is invalid in this context: server unaware of authentication event.");
                return true;
            }
            if (!code.Equals(server.Account.AuthenticationCode))
            {
                ApiError.Throw(ApiErrorCode.InvalidCode, server, "The provided authentication code was incorrect.");
                return true;
            }
            if (server.Account.AuthenticationId != server.RequestId)
            {
                ApiError.Throw(ApiErrorCode.InvalidContext, server, "The requested action is invalid in this context: authentication event does not match.");
                return true;
            }
            if (server.Account.AuthenticationTime + MainServer.Config.WamsrvSecurityConfig.TwoFactorExpirationTime < DatabaseEssentials.GetTimeStamp())
            {
                ApiError.Throw(ApiErrorCode.ExpiredCode, server, "The provided authentication code has expired.");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Throws an exception if the user is online and returns false otherwise.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static bool AssertUserOffline(this ApiServer server)
        {
            if (server.AssertAccountNotNull())
            {
                return true;
            }
            if (server.Account.IsOnline)
            {
                ApiError.Throw(ApiErrorCode.InvalidContext, server, "The requested action is invalid in this context: already logged in.");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Throws an exception if the user is offline and returns false otherwise.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static bool AssertUserOnline(this ApiServer server)
        {
            if (server.AssertAccountNotNull())
            {
                return true;
            }
            if (!server.Account.IsOnline)
            {
                ApiError.Throw(ApiErrorCode.InvalidContext, server, "The requested action is invalid in this context: not logged in.");
                return true;
            }
            return false;
        }
        /// <summary>
        /// Throws an exception if the id is not set and returns false otherwise.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static bool AssertIdSet(this ApiServer server)
        {
            if (server.AssertAccountNotNull())
            {
                return true;
            }
            if (string.IsNullOrEmpty(server.Account.Id))
            {
                ApiError.Throw(ApiErrorCode.InvalidContext, server, "The requested action is invalid in this context: user id not set.");
                return true;
            }
            return false;
        }
    }
}
