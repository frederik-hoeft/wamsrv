using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv
{
    /// <summary>
    /// Account info eventually shared with the clients
    /// </summary>
    public class AccountInfo
    {
        public readonly string Name;
        public readonly string Occupation;
        public readonly string Info;
        public readonly string Location;
        public readonly string UserId;
        public readonly string Email;
        public AccountInfo(string name, string occupation, string info, string location, string userid, string email)
        {
            Name = name;
            Occupation = occupation;
            Info = info;
            Location = location;
            UserId = userid;
            Email = email;
        }
    }
}
