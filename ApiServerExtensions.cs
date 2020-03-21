using wamsrv.ApiRequests;
using wamsrv.ApiResponses;
using wamsrv.Database;

namespace wamsrv
{
    public static class ApiServerExtensions
    {
        public static bool AssertAccountNotNull(this ApiServer server, ApiRequestId requestId)
        {
            if (server.Account != null)
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InvalidContext, requestId, "The requested action is invalid in this context.");
                server.Network.Send(errorCode);
                return true;
            }
            return false;
        }

        public static bool AssertAccountNull(this ApiServer server, ApiRequestId requestId)
        {
            if (server.Account == null)
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InvalidContext, requestId, "The requested action is invalid in this context.");
                server.Network.Send(errorCode);
                return true;
            }
            return false;
        }

        public static bool AssertAuthenticationCode(this ApiServer server, ApiRequestId requestId, string code)
        {
            if (server.AssertAccountNull(requestId))
            {
                return true;
            }
            if (string.IsNullOrEmpty(server.Account.AuthenticationCode))
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InvalidContext, requestId, "The requested action is invalid in this context: server unaware of authentication event.");
                server.Network.Send(errorCode);
                return true;
            }
            if (!code.Equals(server.Account.AuthenticationCode))
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InvalidCode, requestId, "The provided authentication code was incorrect.");
                server.Network.Send(errorCode);
                return true;
            }
            if (server.Account.AuthenticationId != requestId)
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InvalidContext, requestId, "The requested action is invalid in this context: authentication event does not match.");
                server.Network.Send(errorCode);
                return true;
            }
            if (server.Account.AuthenticationTime + MainServer.Config.WamsrvSecurityConfig.TwoFactorExpirationTime < DatabaseEssentials.GetTimeStamp())
            {
                string errorCode = ApiError.Throw(ApiErrorCode.ExpiredCode, requestId, "The provided authentication code has expired.");
                server.Network.Send(errorCode);
                return true;
            }
            return false;
        }

        public static bool AssertUserOnline(this ApiServer server, ApiRequestId requestId)
        {
            if (server.AssertAccountNull(requestId))
            {
                return true;
            }
            if (server.Account.IsOnline)
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InvalidContext, requestId, "The requested action is invalid in this context: already logged in.");
                server.Network.Send(errorCode);
                return true;
            }
            return false;
        }

        public static bool AssertUserOffline(this ApiServer server, ApiRequestId requestId)
        {
            if (server.AssertAccountNull(requestId))
            {
                return true;
            }
            if (!server.Account.IsOnline)
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InvalidContext, requestId, "The requested action is invalid in this context: not logged in.");
                server.Network.Send(errorCode);
                return true;
            }
            return false;
        }
        public static bool AssertIdNotSet(this ApiServer server, ApiRequestId requestId)
        {
            if (server.AssertAccountNull(requestId))
            {
                return true;
            }
            if (string.IsNullOrEmpty(server.Account.Id))
            {
                string errorCode = ApiError.Throw(ApiErrorCode.InvalidContext, requestId, "The requested action is invalid in this context: user id not set.");
                server.Network.Send(errorCode);
                return true;
            }
            return false;
        }
    }
}
