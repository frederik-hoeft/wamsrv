using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv
{
    /// <summary>
    /// Client specific network api (preventing race conditions)
    /// </summary>
    public class Network
    {
        private readonly Client client;
        private readonly Queue<string> networkQueue = new Queue<string>();
        private bool isProcessing = false;
        public Network(Client client)
        {
            this.client = client;
        }

        public void Send(string json)
        {
            networkQueue.Enqueue(json);
            ProcessNetworkQueue();
        }

        private void ProcessNetworkQueue()
        {
            if (isProcessing)
            {
                return;
            }
            while (networkQueue.Count > 0)
            {
                string data = networkQueue.Dequeue();
                client.SslStream.Write(Encoding.UTF8.GetBytes(data));
                client.SslStream.Flush();
            }
            isProcessing = false;
        }
    }
}
