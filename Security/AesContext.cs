namespace wamsrv.Security
{
    public class AesContext
    {
        private readonly string password;
        public AesContext(string userid)
        {
            password = SecurityManager.DeriveUserSecret(userid);
        }

        public string Encrypt(string text)
        {
            return SecurityManager.AESEncrypt(text, password);
        }

        public string Decrypt(string text)
        {
            return SecurityManager.AESDecrypt(text, password);
        }
    }
}
