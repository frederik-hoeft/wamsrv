using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiRequests
{
    /// <summary>
    /// Api request base class
    /// </summary>
    public abstract class ApiRequest
    {
        public ApiRequestId RequestId;

        public abstract void Process(ApiServer server);
    }
}
