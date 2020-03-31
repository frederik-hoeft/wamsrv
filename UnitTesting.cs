﻿using wamsrv.ApiRequests;
using wamsrv.ApiResponses;

namespace wamsrv
{
    public class UnitTesting
    {
        public bool MethodSuccess { get; set; } = false;
        public ApiRequestId RequestId { get; set; } = ApiRequestId.Invalid;
        public ApiErrorCode ErrorCode { get; set; } = ApiErrorCode.Ok;
    }
}