using System;
using System.Collections.Generic;
using System.Text;
using wamsrv.ApiRequests;

namespace wamsrv
{
    public class UnitTesting
    {
        public bool MethodSuccess { get; set; } = false;
        public ApiRequestId RequestId { get; set; } = ApiRequestId.Invalid;

    }
}
