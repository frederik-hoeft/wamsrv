namespace wamsrv
{
    public class WamsrvConfig
    {
        public readonly int LocalPort;
        public readonly string PfxCertificatePath;
        public readonly string PfxPassword;
        public readonly bool SuppressCertificateErrors;
        public readonly WadbsrvInterfaceConfig WadbsrvInterfaceConfig;
        public readonly WadbsrvSecurityConfig WadbsrvSecurityConfig;
        public WamsrvConfig(int localPort, string pfxCertificatePath, string pfxPassword, bool suppressCertificateErrors, WadbsrvInterfaceConfig wadbsrvInterfaceConfig, WadbsrvSecurityConfig wadbsrvSecurityConfig)
        {
            LocalPort = localPort;
            PfxCertificatePath = pfxCertificatePath;
            PfxPassword = pfxPassword;
            SuppressCertificateErrors = suppressCertificateErrors;
            WadbsrvInterfaceConfig = wadbsrvInterfaceConfig;
            WadbsrvSecurityConfig = wadbsrvSecurityConfig;
        }
    }
    public class WadbsrvInterfaceConfig
    {
        public readonly string DatabaseServerIp;
        public readonly int DatabaseServerPort;
        public WadbsrvInterfaceConfig(string databaseServerIp, int databaseServerPort)
        {
            DatabaseServerIp = databaseServerIp;
            DatabaseServerPort = databaseServerPort;
        }
    }
    public class WadbsrvSecurityConfig
    {
        public readonly int ScryptIterationCount;
        public readonly int ScryptBlockSize;
        public readonly int ScryptThreadCount;
        public readonly string ServerSecret;
        public WadbsrvSecurityConfig(int scryptIterationCount, int scryptBlockSize, int scryptThreadCount, string serverSecret)
        {
            ScryptIterationCount = scryptIterationCount;
            ScryptBlockSize = scryptBlockSize;
            ScryptThreadCount = scryptThreadCount;
            ServerSecret = serverSecret;
        }
    }
}
