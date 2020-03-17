using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv
{
    /// <summary>
    /// Account management object
    /// </summary>
    public class Account
    {
        public AccountInfo AccountInfo { get; }
        public bool IsEncrypted { get; set; }
        public Account(AccountInfo info, bool isEncrypted)
        {
            AccountInfo = info;
            IsEncrypted = isEncrypted;
        }
    }
}
