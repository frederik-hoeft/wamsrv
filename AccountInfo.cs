namespace wamsrv
{
    /// <summary>
    /// Account info eventually shared with the clients
    /// </summary>
    public class AccountInfo
    {
        public string Name;
        public string Occupation;
        public string Info;
        public string Location;
        public int Radius;
        public string UserId;
        public string Email;
        public bool IsVisible;
        public bool ShowLog;
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
        public AccountInfo() { }
    }
}
