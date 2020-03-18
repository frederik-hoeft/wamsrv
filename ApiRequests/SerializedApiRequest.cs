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
        public readonly RequestId ApiRequestId;
        public readonly string Json;
        public SerializedApiRequest(RequestId apiRequestId, string json)
        {
            ApiRequestId = apiRequestId;
            Json = json;
        }

        public ApiRequest Deserialize()
        {
            return ApiRequestId switch
            {
                RequestId.CookieValidationRequest => JsonConvert.DeserializeObject<CookieValidationRequest>(Json),
                RequestId.BatchProfileRequest => JsonConvert.DeserializeObject<BatchProfileRequest>(Json),
                _ => null
            };
        }
    }
    public enum RequestId
    {
        CookieValidationRequest = 0,
        BatchProfileRequest = 1,
        CreateEventRequest = 2,
        DeleteEventRequest = 3,
        EditEventRequest = 4,
        GetEventInfoRequest = 5,
        GetEventRequest = 6,
        HandleDislikeEventRequest = 7,
        HandleLikeEventRequest = 8
    }
}
