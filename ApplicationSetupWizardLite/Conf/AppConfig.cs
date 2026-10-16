namespace ApplicationSetupWizardLite.Conf
{
    public class AppConfig
    {
        private readonly XmlHelper _xmlHelper;

        // Installer configuration properties
        public string appName { get; }
        public string appVersion { get; }
        public string companyName { get; }

        // Payload application configuration properties
        public string targetAppName { get; }
        public string targetAppVersion { get; }
        public string targetCompanyName { get; }


        public AppConfig(XmlHelper xmlHelper)
        {
            _xmlHelper = xmlHelper;

            appName = _xmlHelper.GetString("App", "Meta", "AppName");
            appVersion = _xmlHelper.GetString("App", "Meta", "AppVersion");
            companyName = _xmlHelper.GetString("App", "Meta", "CompanyName");

            targetAppName = _xmlHelper.GetString("Payload", "App", "Meta", "AppName");
            targetAppVersion = _xmlHelper.GetString("Payload", "App", "Meta", "AppVersion");
            targetCompanyName = _xmlHelper.GetString("Payload", "App", "Meta", "CompanyName");
        }
    }
}
