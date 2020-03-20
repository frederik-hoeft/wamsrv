using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using washared;

namespace wamsrv
{
    public class SqlClient : NetworkInterface, IDisposable
    {
        public override Network Network { get => base.Network; }
        public override SslStream SslStream { get => base.SslStream; }
        private readonly NetworkStream networkStream;
        private readonly Socket socket;

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None || MainServer.Config.SuppressCertificateErrors)
            {
                return true;
            }

            Debug.WriteLine("Certificate error: {0}", sslPolicyErrors);
            return false;
        }
        public SqlClient(string ip, int port)
        {
            bool success = IPAddress.TryParse(ip, out IPAddress ipAddress);
            if (!success)
            {
                throw new Exception("Unable to parse " + ip);
            }
            IPEndPoint server = new IPEndPoint(ipAddress, port);
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(server);
            networkStream = new NetworkStream(socket);
            Network = new Network(this);
            SslStream = new SslStream(networkStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate));
            X509Certificate2Collection certificates = new X509Certificate2Collection
            {
                MainServer.ServerCertificate
            };
            SslStream.AuthenticateAsClient(string.Empty, certificates, SslProtocols.Tls12, true);
            Debug.WriteLine("Success?");
        }

        public void Dispose()
        {
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
