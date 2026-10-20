using ApplicationSetupWizardLite.Paths;

namespace ApplicationSetupWizardLite.Conf
{
    public class AppConfig
    {
        private readonly XmlHelper _xmlHelper;

        // Installer configuration properties
        public string AppName { get; }
        public string AppVersion { get; }
        public string CompanyName { get; }

        // Payload application configuration properties
        public string TargetAppName { get; }
        public string TargetAppVersion { get; }
        public string TargetCompanyName { get; }
        public string ExecutableName { get; }


        public AppConfig(XmlHelper xmlHelper)
        {
            _xmlHelper = xmlHelper;

            // Installer configuration properties set
            AppName = PathValidator.ValidateSegment(
                _xmlHelper.GetString("App", "Meta", "AppName"),
                nameof(AppName));
            AppVersion = PathValidator.ValidateSegment(
                _xmlHelper.GetString("App", "Meta", "AppVersion"),
                nameof(AppVersion));
            CompanyName = PathValidator.ValidateSegment(
                _xmlHelper.GetString("App", "Meta", "CompanyName"),
                nameof(CompanyName));

            // Payload application configuration properties set 
            TargetAppName = PathValidator.ValidateSegment(
                _xmlHelper.GetString("Payload", "App", "Meta", "AppName"),
                nameof(TargetAppName));
            TargetAppVersion = PathValidator.ValidateSegment(
                _xmlHelper.GetString("Payload", "App", "Meta", "AppVersion"),
                nameof(TargetAppVersion));
            TargetCompanyName = PathValidator.ValidateSegment(
                _xmlHelper.GetString("Payload", "App", "Meta", "CompanyName"),
                nameof(TargetCompanyName));
            ExecutableName = PathValidator.ValidateSegment(
                _xmlHelper.GetString("Payload", "App", "Files", "ExeFile"),
                nameof(ExecutableName));
        }
    }
}
