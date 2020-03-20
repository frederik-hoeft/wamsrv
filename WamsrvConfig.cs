namespace wamsrv
{
    public class WamsrvConfig
    {
        public readonly int LocalPort;
        public readonly string PfxCertificatePath;
        public readonly string PfxPassword;
        public readonly bool SuppressCertificateErrors;
        public readonly WadbsrvInterfaceConfig WadbsrvInterfaceConfig;
        public WamsrvConfig(int localPort, string pfxCertificatePath, string pfxPassword, bool suppressCertificateErrors, WadbsrvInterfaceConfig wadbsrvInterfaceConfig)
        {
            LocalPort = localPort;
            PfxCertificatePath = pfxCertificatePath;
            PfxPassword = pfxPassword;
            SuppressCertificateErrors = suppressCertificateErrors;
            WadbsrvInterfaceConfig = wadbsrvInterfaceConfig;
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
}
