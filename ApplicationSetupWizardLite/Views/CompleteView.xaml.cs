using ApplicationSetupWizardLite.Conf;
using ApplicationSetupWizardLite.Context;
using ApplicationSetupWizardLite.ViewModels;

namespace ApplicationSetupWizardLite.Views
{
    /// <summary>
    /// CompleteView.xaml の相互作用ロジック
    /// </summary>
    public partial class CompleteView : System.Windows.Controls.UserControl
    {
        private readonly AppConfig _appConfig;
        public CompleteView(AppConfig appConfig, SetupResult result)
        {
            InitializeComponent();
            _appConfig = appConfig;
            DataContext = new CompleteViewModel(result);
            // DescriptionText.Text = $"{_appConfig.appName} のインストールが正常に完了しました。";
        }
    }
}
