using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using washared;
using System.Threading.Tasks;
using washared.DatabaseServer.ApiResponses;
using Newtonsoft.Json;
using System.Diagnostics;

namespace wamsrv.Database
{
    /// <summary>
    /// To be used with 'using -> opens connection to local wadbsrv.exe
    /// </summary
    public class DatabaseManager : IDisposable
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
            sqlClient = new SqlClient(MainServer.Config.WadbsrvInterfaceConfig.DatabaseServerIp, MainServer.Config.WadbsrvInterfaceConfig.DatabaseServerPort);
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

        private ApiResponse GetResponseAsync(SqlApiRequest request)
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

        public SqlModifyDataResponse GetModifyDataResponse(SqlApiRequest request)
        {
            return (SqlModifyDataResponse)GetResponseAsync(request);
        }
        public Sql2DArrayResponse Get2DArrayResponse(SqlApiRequest request)
        {
            return (Sql2DArrayResponse)GetResponseAsync(request);
        }
        public SqlDataArrayResponse GetDataArrayResponse(SqlApiRequest request)
        {
            return (SqlDataArrayResponse)GetResponseAsync(request);
        }
        public SqlSingleOrDefaultResponse GetSingleOrDefaultResponse(SqlApiRequest request)
        {
            return (SqlSingleOrDefaultResponse)GetResponseAsync(request);
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
