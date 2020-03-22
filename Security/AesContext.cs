namespace wamsrv.Security
{
    public class AesContext
    {
        private readonly string password;
        public AesContext(string userid)
        {
            password = SecurityManager.DeriveUserSecret(userid);
        }

        public string EncryptOrDefault(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            return SecurityManager.AESEncrypt(text, password);
        }

        public string DecryptOrDefault(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            return SecurityManager.AESDecrypt(text, password);
        }
    }
}
