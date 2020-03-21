using System;

namespace wamsrv.ApiRequests
{
    public class BatchProfileRequest : ApiRequest
    {
        public readonly int BatchSize;
        public BatchProfileRequest(ApiRequestId requestId, int batchSize)
        {
            RequestId = requestId;
            BatchSize = batchSize;
        }
        public override void Process(ApiServer server)
        {
            throw new NotImplementedException();
        }
    }
}
