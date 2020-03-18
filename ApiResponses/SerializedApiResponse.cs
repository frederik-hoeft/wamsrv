using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiResponses
{
    /// <summary>
    /// Api response serialization wrapper class
    /// </summary>
    public class SerializedApiResponse
    {
        public readonly ResponseId ResponseId;
        public readonly string Json;
        private SerializedApiResponse(ResponseId responseId, string json)
        {
            ResponseId = responseId;
            Json = json;
        }

        public static SerializedApiResponse Create(ResponseId responseId, string json)
        {
            return new SerializedApiResponse(responseId, json);
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    public enum ResponseId
    {
        GetEventInfoResponse = 0,
        GetEventResponse = 1
    }
}
