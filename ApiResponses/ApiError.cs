using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using wamsrv.ApiRequests;

namespace wamsrv.ApiResponses
{
    public class ApiError
    {
        public readonly ApiErrorCode ApiErrorCode;
        public readonly ApiRequestId OriginalRequestId;
        public readonly string Message;
        public readonly TargetSite TargetSite;
        private ApiError(ApiErrorCode apiErrorCode, ApiRequestId originalRequestId, string message, TargetSite targetSite)
        {
            ApiErrorCode = apiErrorCode;
            OriginalRequestId = originalRequestId;
            Message = message;
            TargetSite = targetSite;
        }

        public static ApiError Create(ApiErrorCode errorCode, ApiRequestId requestId, string message)
        {
            return new ApiError(errorCode, requestId, message, null);
        }

        public static ApiError Create(ApiErrorCode errorCode, ApiRequestId requestId, string message, TargetSite targetSite)
        {
            return new ApiError(errorCode, requestId, message, targetSite);
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static string Throw(ApiErrorCode errorCode, ApiRequestId requestId, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            TargetSite targetSite = null;
            if (MainServer.Config.DebuggingEnabled)
            {
                targetSite = new TargetSite(memberName, sourceFilePath, sourceLineNumber);
            }
            ApiError apiError = new ApiError(errorCode, requestId, message, targetSite);
            string json = apiError.Serialize();
            Debug.WriteLine(json);
            SerializedApiResponse apiResponse = SerializedApiResponse.Create(ResponseId.Error, json);
            return apiResponse.Serialize();
        }
    }
    public enum ApiErrorCode
    {
        InvalidToken = 0,
        InternalServerError = 1,
        InvalidCredentials = 2,
        UnknownUser = 3,
        AlreadyOnline = 4,
        InvalidEmailAddress = 5,
        InvalidContext = 6,
        InvalidCode = 7,
        ExpiredCode = 8,
        InvalidState = 9,
    }
}
