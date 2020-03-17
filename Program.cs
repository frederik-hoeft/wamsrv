using Newtonsoft.Json;
using System;
using wamsrv.ApiRquests;

namespace wamsrv
{
    class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            CookieValidationRequest validationRequest = new CookieValidationRequest(RequestId.CookieValidationRequest, "diTh+JzgCfjsfm45eBpCwKJ/o4+U208F94sPMBAH4Go=");
            string json = JsonConvert.SerializeObject(validationRequest);
            SerializedApiRequest serializedApiRequest1 = new SerializedApiRequest(RequestId.CookieValidationRequest, json);
            string finalJson = JsonConvert.SerializeObject(serializedApiRequest1);
            Console.WriteLine(finalJson);
            SerializedApiRequest serializedApiRequest = JsonConvert.DeserializeObject<SerializedApiRequest>(finalJson);
            ApiRequest request = serializedApiRequest.Deserialize();
            request.Process();
            // MainServer.Run();
        }
    }
}
