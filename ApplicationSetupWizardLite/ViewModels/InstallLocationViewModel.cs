using ApplicationSetupWizardLite.Context;

namespace ApplicationSetupWizardLite.ViewModels
{
    public class InstallLocationViewModel
    {
        private readonly SetupContext _setupContext;
        public string InstallPath { get; set; } = "";
        public string FullInstallPath { get; set; } = "";
        public string PLText { get; set; } = "";
        public bool CreateShortcut { get; set; } = true;
        public string StatusText { get; set; } = "";

        public InstallLocationViewModel(SetupContext setupContext)
        {
            _setupContext = setupContext;
            InstallPath = _setupContext.InstallPath;
            CreateShortcut = _setupContext.CreateShortcut;
        }
        public void UpdateContext()
        {
            _setupContext.InstallPath = InstallPath;
            _setupContext.CreateShortcut = CreateShortcut;
        }
    }
}
