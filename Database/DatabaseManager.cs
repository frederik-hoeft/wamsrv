using Newtonsoft.Json;
using System;
using System.Diagnostics;
using wamsrv.ApiResponses;
using washared;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;
using ApiResponse = washared.DatabaseServer.ApiResponses.ApiResponse;
using SerializedApiResponse = washared.DatabaseServer.ApiResponses.SerializedApiResponse;

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

        private ApiResponse GetResponse(SqlApiRequest request)
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
                Debug.WriteLine(jsonResponse);
                SerializedApiResponse serializedApiResponse = JsonConvert.DeserializeObject<SerializedApiResponse>(jsonResponse);
                response = serializedApiResponse.Deserialize();
            }
            if (response == null)
            {
                ApiError.Throw(ApiErrorCode.InternalServerError, server, "Database request timed out.");
            }
            return response;
        }

        public SqlModifyDataResponse AwaitModifyDataResponse(SqlApiRequest request, out bool success)
        {
            ApiResponse apiResponse = GetResponse(request);
            if (apiResponse == null)
            {
                success = false;
                return null;
            }
            if (apiResponse.ResponseId == SqlResponseId.Error)
            {
                SqlErrorResponse sqlError = (SqlErrorResponse)apiResponse;
                ApiError.Throw(ApiErrorCode.DatabaseException, server, sqlError.Message);
                success = false;
                return null;
            }
            success = true;
            return (SqlModifyDataResponse)apiResponse;
        }
        public Sql2DArrayResponse Await2DArrayResponse(SqlApiRequest request, out bool success)
        {
            ApiResponse apiResponse = GetResponse(request);
            if (apiResponse == null)
            {
                success = false;
                return null;
            }
            if (apiResponse.ResponseId == SqlResponseId.Error)
            {
                SqlErrorResponse sqlError = (SqlErrorResponse)apiResponse;
                ApiError.Throw(ApiErrorCode.DatabaseException, server, sqlError.Message);
                success = false;
                return null;
            }
            success = true;
            return (Sql2DArrayResponse)apiResponse;
        }
        public SqlDataArrayResponse AwaitDataArrayResponse(SqlApiRequest request, out bool success)
        {
            ApiResponse apiResponse = GetResponse(request);
            if (apiResponse == null)
            {
                success = false;
                return null;
            }
            if (apiResponse.ResponseId == SqlResponseId.Error)
            {
                SqlErrorResponse sqlError = (SqlErrorResponse)apiResponse;
                ApiError.Throw(ApiErrorCode.DatabaseException, server, sqlError.Message);
                success = false;
                return null;
            }
            success = true;
            return (SqlDataArrayResponse)apiResponse;
        }
        public SqlSingleOrDefaultResponse AwaitSingleOrDefaultResponse(SqlApiRequest request, out bool success)
        {
            ApiResponse apiResponse = GetResponse(request);
            if (apiResponse == null)
            {
                success = false;
                return null;
            }
            if (apiResponse.ResponseId == SqlResponseId.Error)
            {
                SqlErrorResponse sqlError = (SqlErrorResponse)apiResponse;
                ApiError.Throw(ApiErrorCode.DatabaseException, server, sqlError.Message);
                success = false;
                return null;
            }
            success = true;
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
