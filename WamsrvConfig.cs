namespace wamsrv
{
    public class WamsrvConfig
    {
        public readonly bool DebuggingEnabled;
        public readonly int LocalPort;
        public readonly string PfxCertificatePath;
        public readonly string PfxPassword;
        public readonly bool SuppressCertificateErrors;
        public readonly WamsrvInterfaceConfig WamsrvInterfaceConfig;
        public readonly WamsrvSecurityConfig WamsrvSecurityConfig;
        public readonly WamsrvEmailConfig WamsrvEmailConfig;
        public WamsrvConfig(bool debuggingEnabled, int localPort, string pfxCertificatePath, string pfxPassword, bool suppressCertificateErrors, WamsrvInterfaceConfig wadbsrvInterfaceConfig, WamsrvSecurityConfig wadbsrvSecurityConfig, WamsrvEmailConfig wamsrvEmailConfig)
        {
            DebuggingEnabled = debuggingEnabled;
            LocalPort = localPort;
            PfxCertificatePath = pfxCertificatePath;
            PfxPassword = pfxPassword;
            SuppressCertificateErrors = suppressCertificateErrors;
            WamsrvInterfaceConfig = wadbsrvInterfaceConfig;
            WamsrvSecurityConfig = wadbsrvSecurityConfig;
            WamsrvEmailConfig = wamsrvEmailConfig;
        }
    }
    public class WamsrvInterfaceConfig
    {
        public readonly string DatabaseServerIp;
        public readonly int DatabaseServerPort;
        public WamsrvInterfaceConfig(string databaseServerIp, int databaseServerPort)
        {
            DatabaseServerIp = databaseServerIp;
            DatabaseServerPort = databaseServerPort;
        }
    }
    public class WamsrvSecurityConfig
    {
        public readonly int ScryptIterationCount;
        public readonly int ScryptBlockSize;
        public readonly int ScryptThreadCount;
        public readonly int TwoFactorExpirationTime;
        public readonly int SecurityTokenExpirationTime;
        public readonly string ServerSecret;
        public WamsrvSecurityConfig(int scryptIterationCount, int scryptBlockSize, int scryptThreadCount, int twoFactorExpirationTime, int securityTokenExpirationTime, string serverSecret)
        {
            ScryptIterationCount = scryptIterationCount;
            ScryptBlockSize = scryptBlockSize;
            ScryptThreadCount = scryptThreadCount;
            ServerSecret = serverSecret;
            TwoFactorExpirationTime = twoFactorExpirationTime;
            SecurityTokenExpirationTime = securityTokenExpirationTime;
        }
    }

    public class WamsrvEmailConfig
    {
        public readonly string EmailDisplayName;
        public readonly string EmailAddress;
        public readonly string EmailPassword;
        public readonly string SmptServer;
        public readonly int SmptServerPort;
        public WamsrvEmailConfig(string emailDisplayName, string emailAddress, string emailPassword, string smptServer, int smptServerPort)
        {
            EmailDisplayName = emailDisplayName;
            EmailAddress = emailAddress;
            EmailPassword = emailPassword;
            SmptServer = smptServer;
            SmptServerPort = smptServerPort;
        }
    }
}
