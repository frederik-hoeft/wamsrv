using wamsrv.ApiRequests;

namespace wamsrv
{
    /// <summary>
    /// Account management object
    /// </summary>
    public class Account
    {
        public AccountInfo AccountInfo { get; set; }
        public bool IsEncrypted { get; set; }
        public string AuthenticationCode { get; set; } = string.Empty;
        public ApiRequestId AuthenticationId { get; set; } = ApiRequestId.Invalid;
        public int AuthenticationTime { get; set; } = -1;
        public string Password { get; set; } = string.Empty;
        public bool IsOnline { get; set; } = false;
        public string Id { get; set; } = string.Empty;
        public Account(AccountInfo info, bool isEncrypted, string id)
        {
            AccountInfo = info;
            IsEncrypted = isEncrypted;
            Id = id;
        }
        public Account() { }
    }
}
