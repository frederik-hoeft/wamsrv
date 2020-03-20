using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using washared;

namespace wamsrv
{
    /// <summary>
    /// Main server loop accepting connections
    /// </summary>
    public static class MainServer
    {
        public static WamsrvConfig Config;
        public static int ClientCount = 0;
        public static X509Certificate2 ServerCertificate;
        public static void Run()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = washared.Extensions.GetLocalIPAddress();
            if (ipAddress == null)
            {
                Console.WriteLine("Unable to resolve IP address.");
                return;
            }
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Config.LocalPort);
            using Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Listen(16);
            while (true)
            {
                Socket clientSocket = socket.Accept();
                ClientCount++;
                new Thread(() => ApiServer.Create(clientSocket)).Start();
            }
        }

        public static void LoadConfig()
        {
            string config = File.ReadAllText("wamsrv.config.json");
            Config = JsonConvert.DeserializeObject<WamsrvConfig>(config);
            ServerCertificate = new X509Certificate2(Config.PfxCertificatePath, Config.PfxPassword);
        }
    }
}
