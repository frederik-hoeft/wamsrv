using Newtonsoft.Json;
using System;
using System.Diagnostics;
using washared;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.Database
{
    /// <summary>
    /// To be used with 'using -> opens connection to local wadbsrv.exe
    /// </summary
    public partial class DatabaseManager : IDisposable
    {
        private SqlClient sqlClient = null;
        private bool isInitialized = false;
        private PacketParser packetParser = null;

        private void Inititalize()
        {
            if (isInitialized)
            {
                return;
            }
            sqlClient = new SqlClient(MainServer.Config.WamsrvInterfaceConfig.DatabaseServerIp, MainServer.Config.WamsrvInterfaceConfig.DatabaseServerPort);
            packetParser = new PacketParser(sqlClient)
            {
                Interactive = true,
                PacketActionCallback = null,
                ReleaseResources = true,
                UseBackgroundParsing = true,
                PacketTimeoutMillis = 10000
            };
            packetParser.BeginParse();
            isInitialized = true;
        }

        private ApiResponse GetResponse(SqlApiRequest request)
        {
            Inititalize();
            string jsonRequest = request.Serialize();
            sqlClient.Network.Send(jsonRequest);
            byte[] packet = packetParser.GetPacket();
            string jsonResponse = sqlClient.Network.Encoding.GetString(packet);
            Debug.WriteLine(jsonResponse);
            SerializedApiResponse serializedApiResponse = JsonConvert.DeserializeObject<SerializedApiResponse>(jsonResponse);
            return serializedApiResponse.Deserialize();
        }

        public SqlModifyDataResponse AwaitModifyDataResponse(SqlApiRequest request)
        {
            return (SqlModifyDataResponse)GetResponse(request);
        }
        public Sql2DArrayResponse Await2DArrayResponse(SqlApiRequest request)
        {
            return (Sql2DArrayResponse)GetResponse(request);
        }
        public SqlDataArrayResponse AwaitDataArrayResponse(SqlApiRequest request)
        {
            return (SqlDataArrayResponse)GetResponse(request);
        }
        public SqlSingleOrDefaultResponse AwaitSingleOrDefaultResponse(SqlApiRequest request)
        {
            return (SqlSingleOrDefaultResponse)GetResponse(request);
        }

        public void Dispose()
        {
            packetParser.ShutdownAsync();
            if (sqlClient != null)
            {
                try
                {
                    sqlClient.Dispose();
                }
                catch (ObjectDisposedException) { }
            }
        }
    }
}
