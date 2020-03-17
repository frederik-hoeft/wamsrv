using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using wamsrv.ApiRquests;

namespace wamsrv
{
    /// <summary>
    /// Main client handling looper thread
    /// </summary>
    public sealed class Client: IDisposable
    {
        public Network Network { get; }
        public SslStream SslStream { get; }
        private readonly NetworkStream networkStream;
        private readonly Socket socket;
        #region Constructor
        private Client(Socket socket)
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
            Client client = new Client(socket);
            client.Serve();
        }
#nullable disable
        #endregion
        #region Getters / Setters
        #endregion
        private void Serve()
        {
            // Initialize buffer for huge packets (>32 kb)
            List<byte> buffer = new List<byte>();
            // Initialize 32 kb receive buffer for incoming data
            int bufferSize = 32768;
            byte[] data = new byte[bufferSize];
            // Run until thread is terminated
            while (true)
            {
                bool receiving = true;
                // Initialize list to store all packets found in receive buffer
                List<byte[]> dataPackets = new List<byte[]>();
                while (receiving)
                {
                    // Receive and dump to buffer until EOT flag (used to terminate packets in custom protocol --> hex value 0x04) is found
                    int connectionDropped = SslStream.Read(data);
                    if (connectionDropped == 0)
                    {
                        // Break endless loop and free resources
                        Dispose();
                        return;
                    }
                    // ----------------------------------------------------------------
                    //      HANDLE CASES OF MORE THAN ONE PACKET IN RECEIVE BUFFER
                    // ----------------------------------------------------------------
                    // Remove any null bytes from buffer
                    data = data.Where(b => b != 0x00).ToArray();
                    // Check if packet contains EOT flag and if the buffer for big packets is empty
                    if (data.Contains<byte>(0x04) && buffer.Count == 0)
                    {
                        // Split packets on EOT flag (might be more than one packet)
                        List<byte[]> rawDataPackets = data.Separate(new byte[] { 0x04 });
                        // Grab the last packet
                        byte[] lastDataPacket = rawDataPackets[rawDataPackets.Count - 1];
                        // Move all but the last packet into the 2d packet array list
                        List<byte[]> tempRawDataPackets = new List<byte[]>(rawDataPackets);
                        tempRawDataPackets.Remove(tempRawDataPackets.Last());
                        dataPackets = new List<byte[]>(tempRawDataPackets);
                        // In case the last packet contains data too --> move it in buffer for next "receiving round"
                        if (lastDataPacket.Length != 0 && lastDataPacket.Any(b => b != 0))
                        {
                            buffer.AddRange(new List<byte>(lastDataPacket));
                        }
                        // Stop receiving and break the loop
                        receiving = false;
                    }
                    // Check if packet contains EOT flag and the buffer is not empty
                    else if (data.Contains<byte>(0x04) && buffer.Count != 0)
                    {
                        // Split packets on EOT flag (might be more than one packet)
                        List<byte[]> rawDataPackets = data.Separate(new byte[] { 0x04 });
                        // Append content of buffer to the first packet
                        List<byte> firstPacket = new List<byte>();
                        firstPacket.AddRange(buffer);
                        firstPacket.AddRange(new List<byte>(rawDataPackets[0]));
                        rawDataPackets[0] = firstPacket.ToArray();
                        // Reset the buffer
                        buffer = new List<byte>();
                        // Grab the last packet
                        byte[] lastDataPacket = rawDataPackets[rawDataPackets.Count - 1];
                        // Move all but the last packet into the 2d packet array list
                        List<byte[]> tempRawDataPackets = new List<byte[]>(rawDataPackets);
                        tempRawDataPackets.Remove(tempRawDataPackets.Last());
                        dataPackets = new List<byte[]>(tempRawDataPackets);
                        // In case the last packet contains data too --> move it in buffer for next "receiving round"
                        if (lastDataPacket.Length != 0 && lastDataPacket.Any(b => b != 0))
                        {
                            buffer.AddRange(new List<byte>(lastDataPacket));
                        }
                        // Stop receiving and break the loop
                        receiving = false;
                    }
                    // The buffer does not contain any EOT flag
                    else
                    {
                        // Damn that's a huge packet. append the whole thing to the buffer and repeat until EOT flag is found
                        buffer.AddRange(new List<byte>(data));
                    }
                    // Reset the data buffer
                    data = new byte[bufferSize];
                }
                for (int i = 0; i < dataPackets.Count; i++)
                {
                    byte[] packet = dataPackets[i];
                    // Check if packets have a valid entrypoint / start of heading
                    if (packet[0] != 0x01)
                    {
                        // Check if there's a valid entry point in the packet
                        if (packet.Where(currentByte => currentByte.Equals(0x01)).Count() == 1)
                        {
                            int index = Array.IndexOf(packet, 0x01);
                            byte[] temp = new byte[packet.Length - index];
                            Array.Copy(packet, index, temp, 0, packet.Length - index);
                            packet = temp;
                        }
                        // This packet is oficially broken (containing several entry points). Hope that it wasn't too important and continue with the next one
                        else
                        {
                            continue;
                        }
                    }
                    if (packet.Length <= 1)
                    {
                        continue;
                    }
                    // Remove entry point marker byte (0x01)
                    string json = Encoding.UTF8.GetString(packet,1, packet.Length - 1);
                    SerializedApiRequest serializedApiRequest = JsonConvert.DeserializeObject<SerializedApiRequest>(json);
                    ApiRequest apiRequest = serializedApiRequest.Deserialize();
                    apiRequest.Process(this);
                }
            }
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
