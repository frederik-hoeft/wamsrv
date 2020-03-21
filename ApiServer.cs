using Newtonsoft.Json;
using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using wamsrv.ApiRequests;
using washared;

namespace wamsrv
{
    /// <summary>
    /// Main client handling looper thread
    /// </summary>
    public sealed class ApiServer : NetworkInterface, IDisposable
    {
        public override Network Network { get => base.Network; }
        public override SslStream SslStream { get => base.SslStream; }
        public Account Account { get; set; } = null;
        private readonly NetworkStream networkStream;
        private readonly Socket socket;
        #region Constructor
        private ApiServer(Socket socket)
        {
            this.socket = socket;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 10);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 5);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 6);
            networkStream = new NetworkStream(socket);
            SslStream = new SslStream(networkStream);
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

        public void Dispose()
        {
            MainServer.ClientCount--;
            try
            {
                SslStream.Close();
                SslStream.Dispose();
            }
            catch (ObjectDisposedException) { }
            try
            {
                networkStream.Close();
                networkStream.Dispose();
            }
            catch (ObjectDisposedException) { }
            try
            {
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Disconnect(false);
                }
                socket.Close();
            }
            catch (ObjectDisposedException) { }
        }
    }
}
