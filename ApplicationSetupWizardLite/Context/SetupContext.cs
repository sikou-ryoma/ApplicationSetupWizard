using ApplicationSetupWizardLite.Conf;

namespace ApplicationSetupWizardLite.Context

{
    public class SetupContext
    {
        private readonly AppConfig _appConfig;
        public string InstallPath { get; set; }
        public string PayloadSizeText { get; set; }
        public bool CreateShortcut { get; set; }

        public SetupContext(AppConfig appConfig)
        {
            _appConfig = appConfig;
            InstallPath = "";
            PayloadSizeText = "";
            CreateShortcut = true;
        }
        
        public string GetFullInstallPath(string baseInstallPath = "")
        {
            if (string.IsNullOrEmpty(baseInstallPath))
            {
                baseInstallPath = InstallPath;
            }

            return System.IO.Path.Combine(
                baseInstallPath,
                _appConfig.TargetCompanyName,
                _appConfig.TargetAppName,
                _appConfig.TargetAppVersion
            );
        }
    }
}
