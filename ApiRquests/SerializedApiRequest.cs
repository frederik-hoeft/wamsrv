using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using wamsrv.ApiRquests;

namespace wamsrv.ApiRquests
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
                _ => null
            };
        }
    }
    public enum RequestId
    {
        CookieValidationRequest = 0
    }
}
