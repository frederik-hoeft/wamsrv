using Newtonsoft.Json;

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

        public static SerializedApiResponse Create(ApiResponse apiResponse)
        {
            return new SerializedApiResponse(apiResponse.ResponseId, apiResponse.Serialize());
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    public enum ResponseId
    {
        Error = -1,
        GetEventInfoResponse = 0,
        GetEventResponse = 1,
        CreateCookie = 2,
        CreateAccount = 3,
        ActivateAccount = 4,
        UpdateAccountInfo = 5,
    }
}
