using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace wamsrv.ApiRequests
{
    public class BatchProfileRequest : ApiRequest
    {
        public readonly int BatchSize;
        public BatchProfileRequest(RequestId requestId, int batchSize)
        {
            RequestId = requestId;
            BatchSize = batchSize;
        }
        public override void Process(ApiClient client)
        {
            throw new NotImplementedException();
        }
    }
}
