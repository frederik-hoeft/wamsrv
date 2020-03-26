﻿using Newtonsoft.Json;
using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using wamsrv.ApiRequests;
using wamsrv.Database;
using washared;

namespace wamsrv
{
    /// <summary>
    /// Main client handling looper thread
    /// </summary>
    public sealed class ApiServer : DisposableNetworkInterface
    {
        public override Network Network { get => base.Network; }
        public override SslStream SslStream { get => base.SslStream; }
        public Account Account { get; set; } = null;
        public readonly UnitTesting UnitTesting = new UnitTesting();
        private ApiRequestId requestId = ApiRequestId.Invalid;
        public ApiRequestId RequestId
        {
            get { return requestId; }
            set
            {
                requestId = value;
                UnitTesting.RequestId = value;
            }
        }
        #region Constructor
        private ApiServer() : base(null)
        {
            Network = new Network(this);
        }
        private ApiServer(Socket socket) : base(socket)
        {
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 10);
            Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 5);
            Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 6);
            SslStream = new SslStream(NetworkStream);
            SslStream.AuthenticateAsServer(MainServer.ServerCertificate, false, System.Security.Authentication.SslProtocols.Tls12, false);
            Network = new Network(this);
        }
#nullable enable
        public static void Create(Socket? socket)
        {
            if (socket == null)
            {
                MainServer.ClientCount--;
                return;
            }
            ApiServer server = new ApiServer(socket);
            server.Serve();
        }

        public void Send(string data)
        {
            if (!UnitTestDetector.IsInUnitTest && !MainServer.Config.WamsrvDevelopmentConfig.BlockResponses)
            {
                Network.Send(data);
            }
        }

        public static ApiServer CreateDummy()
        {
            return new ApiServer();
        }
#nullable disable
        #endregion
        #region Getters / Setters
        #endregion
        private void Serve()
        {
            using PacketParser parser = new PacketParser(this)
            {
                PacketActionCallback = PacketActionCallback,
                UseMultiThreading = true,
                ReleaseResources = true,
                Interactive = false
            };
            try
            {
                parser.BeginParse();
            }
            catch (ConnectionDroppedException)
            {
                parser.Dispose();
                Dispose();
            }
        }

        private void PacketActionCallback(byte[] packet)
        {
            string json = Encoding.UTF8.GetString(packet);
            SerializedApiRequest serializedApiRequest = JsonConvert.DeserializeObject<SerializedApiRequest>(json);
            ApiRequest apiRequest = serializedApiRequest.Deserialize();
            apiRequest.Process(this);
        }

        public override void Dispose()
        {
            try
            {
                if (!string.IsNullOrEmpty(Account.Id))
                {
                    using DatabaseManager databaseManager = new DatabaseManager(this);
                    databaseManager.SetUserOffline();
                }
            }
            catch { }
            MainServer.ClientCount--;
            base.Dispose();
        }
    }
}
