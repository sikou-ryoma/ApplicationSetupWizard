using ApplicationSetupWizardLite.Conf;
using ApplicationSetupWizardLite.Paths;
using ApplicationSetupWizardLite.Context;
using System.IO;

namespace ApplicationSetupWizardLite.Views
{
    /// <summary>
    /// ConfirmView.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfirmView : System.Windows.Controls.UserControl
    {
        private readonly AppConfig _appConfig;
        private SetupContext _setupContext;
        public ConfirmView(AppConfig appConfig, SetupContext setupCtx)
        {
            InitializeComponent();
            _appConfig = appConfig;
            _setupContext = setupCtx;
            ApplicationNameText.Text = _appConfig.TargetAppName;
            RequiredSpaceText.Text = _setupContext.PayloadSizeText;
            UpdateConfirmView();
        }
        public void UpdateConfirmView()
        {
            InstallPathText.Text = Path.Combine(
                    _setupContext.InstallPath,
                    _appConfig.TargetCompanyName,
                    _appConfig.TargetAppName,
                    _appConfig.TargetAppVersion
            );

            ShortcutText.Text = _setupContext.CreateShortcut ? "ショートカットを作成する" : "作成しない";
        }
    }
}
