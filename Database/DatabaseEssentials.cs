﻿using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.Database
{
    public static partial class DatabaseEssentials
    {
        public static int GetTimeStamp()
        {
            return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
        // TODO: Implement
        public static int UserIdToDatabaseId(string userid)
        {
            return 0;
        }
        /// <summary>
        /// Provides database security related methods.
        /// </summary>
        public static class Security
        {
            /// <summary>
            /// Turns an unsafe string into a SQL injection safe string.
            /// </summary>
            /// <param name="unsafeString">The string to check.</param>
            /// <returns>SQLI safe string.</returns>
            public static string Sanitize(string unsafeString)
            {
                return unsafeString.Replace("\'", "\'\'").Replace("\"", "\"\"");
            }
            /// <summary>
            /// Turns an unsafe SQL query into a SQL injection safe query.
            /// </summary>
            /// <param name="unsafeQuery">The SQL query to check. NOTE: Every even idex is considered to be a SQL statement.</param>
            /// <returns>SQLI safe query.</returns>
            public static string SanitizeQuery(string[] unsafeQuery)
            {
                int queryLength = unsafeQuery.Length;
                if (queryLength == 0 || queryLength % 2 == 0)
                {
                    throw new ArgumentException("There must be an odd number of arguments.");
                }
                for (int i = 1; i < queryLength; i += 2)
                {
                    unsafeQuery[i] = Sanitize(unsafeQuery[i]);
                }
                string query = string.Empty;
                for (int i = 0; i < queryLength; i++)
                {
                    query += unsafeQuery[i];
                }
                return query;
            }
        }
    }
}
