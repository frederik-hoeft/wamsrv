﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

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