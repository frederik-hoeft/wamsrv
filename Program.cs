using Newtonsoft.Json;
using System;
using wamsrv.ApiRequests;
using wamsrv.ApiResponses;

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
            EventInfo eventInfo = new EventInfo("SomeTitle", 123446578, "20-02-2042", "12:00", "Antarctica", "https://example.com", "", "");
            GetEventInfoResponse getEventInfoResponse = GetEventInfoResponse.Create(ResponseId.GetEventInfoResponse, eventInfo);
            string json = getEventInfoResponse.Serialize();
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(ResponseId.GetEventInfoResponse, json);
            string jsonFinal = serializedApiResponse.Serialize();
            Console.WriteLine(jsonFinal);
            // MainServer.Run();
        }
    }
}
