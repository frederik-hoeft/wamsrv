﻿using Newtonsoft.Json;

namespace wamsrv.ApiResponses
{
    /// <summary>
    /// Api response base class
    /// </summary>
    public abstract class ApiResponse
    {
        public ResponseId ResponseId;

        /// <summary>
        /// Serializes the current object to a Json string.
        /// </summary>
        /// <returns>The current object as a JSON string.</returns>
        public virtual string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
