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
        public readonly int Radius;
        public readonly string UserId;
        public readonly string Email;
        public readonly bool IsVisible;
        public readonly bool ShowLog;
        public AccountInfo(string name, string occupation, string info, string location, int radius, string userid, string email, bool isVisible, bool showLog)
        {
            Name = name;
            Occupation = occupation;
            Info = info;
            Location = location;
            Radius = radius;
            UserId = userid;
            Email = email;
            IsVisible = isVisible;
            ShowLog = showLog;
        }
    }
}
