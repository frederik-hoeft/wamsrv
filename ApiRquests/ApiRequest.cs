using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.ApiRquests
{
    /// <summary>
    /// Api request base class
    /// </summary>
    public abstract class ApiRequest
    {
        public RequestId RequestId;

        public abstract void Process(Client client);
    }
}
