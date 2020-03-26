namespace wamsrv.Config
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
        public readonly WamsrvDevelopmentConfig WamsrvDevelopmentConfig;
        public WamsrvConfig(bool debuggingEnabled, int localPort, string pfxCertificatePath, string pfxPassword, bool suppressCertificateErrors, WamsrvInterfaceConfig wamsrvInterfaceConfig, WamsrvSecurityConfig wamsrvSecurityConfig, WamsrvEmailConfig wamsrvEmailConfig, WamsrvDevelopmentConfig wamsrvDevelopmentConfig)
        {
            DebuggingEnabled = debuggingEnabled;
            LocalPort = localPort;
            PfxCertificatePath = pfxCertificatePath;
            PfxPassword = pfxPassword;
            SuppressCertificateErrors = suppressCertificateErrors;
            WamsrvInterfaceConfig = wamsrvInterfaceConfig;
            WamsrvSecurityConfig = wamsrvSecurityConfig;
            WamsrvEmailConfig = wamsrvEmailConfig;
            WamsrvDevelopmentConfig = wamsrvDevelopmentConfig;
        }
    }
}
