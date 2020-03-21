using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiRequests;

namespace wamsrv.Database
{
    /// <summary>
    /// SQL API Request wrapper class. Can hold any SQL API Request.
    /// </summary>
    public class SqlApiRequest : ApiRequestBase
    {
        private SqlApiRequest(SqlRequestId sqlRequestId, string query, int expectedColumns)
        {
            RequestId = sqlRequestId;
            Query = query;
            ExpectedColumns = expectedColumns;
        }

        public static SqlApiRequest Create(SqlRequestId sqlRequestId, string query, int expectedColumns)
        {
            return new SqlApiRequest(sqlRequestId, query, expectedColumns);
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
