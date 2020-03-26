using System;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using washared;

namespace wamsrv
{
    public class SqlClient : DisposableNetworkInterface
    {
        public override Network Network { get => base.Network; }
        public override SslStream SslStream { get => base.SslStream; }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None || MainServer.Config.SuppressCertificateErrors)
            {
                return true;
            }

            Debug.WriteLine("Certificate error: {0}", sslPolicyErrors);
            return false;
        }
        public SqlClient(string ip, int port) : base(ip, port)
        {
            Network = new Network(this);
            SslStream = new SslStream(NetworkStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate));
            X509Certificate2Collection certificates = new X509Certificate2Collection
            {
                MainServer.ServerCertificate
            };
            SslStream.AuthenticateAsClient(string.Empty, certificates, SslProtocols.None, true);
        }
    }
}
