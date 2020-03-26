using Newtonsoft.Json;
using System;
using System.Diagnostics;
using wamsrv.ApiResponses;
using washared;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;
using ApiResponse = washared.DatabaseServer.ApiResponses.ApiResponse;

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
        private readonly ApiServer server;

        public DatabaseManager(ApiServer server)
        {
            this.server = server;
        }

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

        private ApiResponse GetResponse(SqlApiRequest request, out bool success)
        {
            Inititalize();
            string jsonRequest = request.Serialize();
            sqlClient.Network.Send(jsonRequest);
            byte[] packet = packetParser.GetPacket();
            ApiResponse response;
            if (packet.Length == 0)
            {
                response = null;
            }
            else
            {
                string jsonResponse = sqlClient.Network.Encoding.GetString(packet);
                Debug.WriteLine(">> " + jsonResponse);
                SerializedSqlApiResponse serializedApiResponse = JsonConvert.DeserializeObject<SerializedSqlApiResponse>(jsonResponse);
                response = serializedApiResponse.Deserialize();
            }
            if (response == null)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Database request timed out.");
                success = false;
                return null;
            }
            if (response.ResponseId == SqlResponseId.Error)
            {
                SqlErrorResponse sqlError = (SqlErrorResponse)response;
                ApiError.Throw(ApiErrorCode.DatabaseException, server, sqlError.Message);
                success = false;
                return null;
            }
            success = true;
            return response;
        }

        public SqlModifyDataResponse AwaitModifyDataResponse(SqlApiRequest request, out bool success)
        {
            ApiResponse apiResponse = GetResponse(request, out bool sqlSuccess);
            success = sqlSuccess;
            return (SqlModifyDataResponse)apiResponse;
        }
        public Sql2DArrayResponse Await2DArrayResponse(SqlApiRequest request, out bool success)
        {
            ApiResponse apiResponse = GetResponse(request, out bool sqlSuccess);
            success = sqlSuccess;
            return (Sql2DArrayResponse)apiResponse;
        }
        public SqlDataArrayResponse AwaitDataArrayResponse(SqlApiRequest request, out bool success)
        {
            ApiResponse apiResponse = GetResponse(request, out bool sqlSuccess);
            success = sqlSuccess;
            return (SqlDataArrayResponse)apiResponse;
        }
        public SqlSingleOrDefaultResponse AwaitSingleOrDefaultResponse(SqlApiRequest request, out bool success)
        {
            ApiResponse apiResponse = GetResponse(request, out bool sqlSuccess);
            success = sqlSuccess;
            return (SqlSingleOrDefaultResponse)apiResponse;
        }

        public void Dispose()
        {
            if (!isInitialized)
            {
                return;
            }
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
    public enum SqlErrorState
    {
        Success,
        SqlError,
        GenericError
    }
}
