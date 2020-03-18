using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using wamsrv.ApiRequests;

namespace wamsrv.ApiRequests
{
    /// <summary>
    /// Api request serialization wrapper class
    /// </summary>
    public class SerializedApiRequest
    {
        public readonly ApiRequestId ApiRequestId;
        public readonly string Json;
        public SerializedApiRequest(ApiRequestId apiRequestId, string json)
        {
            ApiRequestId = apiRequestId;
            Json = json;
        }

        public ApiRequest Deserialize()
        {
            return ApiRequestId switch
            {
                ApiRequestId.CookieValidation => JsonConvert.DeserializeObject<CookieValidationRequest>(Json),
                ApiRequestId.BatchProfile => JsonConvert.DeserializeObject<BatchProfileRequest>(Json),
                _ => null
            };
        }
    }
    public enum ApiRequestId
    {
        CookieValidation = 0,
        BatchProfile = 1,
        CreateEvent = 2,
        DeleteEvent = 3,
        EditEvent = 4,
        GetEventInfo = 5,
        GetEvent = 6,
        HandleDislikeEvent = 7,
        HandleLikeEvent = 8
    }
}
