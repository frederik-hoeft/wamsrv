using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using wamsrv.ApiRequests;
using washared;

namespace wamsrv
{
    /// <summary>
    /// Main client handling looper thread
    /// </summary>
    public sealed class ApiClient: Client, IDisposable
    {
        public override Network Network { get => base.Network; }
        public override SslStream SslStream { get => base.SslStream; }
        private readonly NetworkStream networkStream;
        private readonly Socket socket;
        #region Constructor
        private ApiClient(Socket socket)
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
            ApiClient client = new ApiClient(socket);
            client.Serve();
        }
#nullable disable
        #endregion
        #region Getters / Setters
        #endregion
        private void Serve()
        {
            using PacketParser packetParser = new PacketParser(this);
            try
            {
                packetParser.BeginParse(PacketActionCallback);
            }
            catch (ConnectionDroppedException)
            {
                packetParser.Dispose();
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
