using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace wamsrv.ApiRquests
{
    public class BatchProfileRequest : ApiRequest
    {
        public readonly int BatchSize;
        public BatchProfileRequest(RequestId requestId, int batchSize)
        {
            RequestId = requestId;
            BatchSize = batchSize;
        }
        public override void Process(Client client)
        {
            new Thread(() => Worker(client)).Start();
        }
        private void Worker(Client client)
        {

        }
    }
}
