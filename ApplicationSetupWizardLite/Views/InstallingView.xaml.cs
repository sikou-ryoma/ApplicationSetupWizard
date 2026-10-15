using ApplicationSetupWizardLite.Conf;
using ApplicationSetupWizardLite.ViewModels;

namespace ApplicationSetupWizardLite.Views
{
    /// <summary>
    /// InstallingView.xaml の相互作用ロジック
    /// </summary>
    public partial class InstallingView : System.Windows.Controls.UserControl
    {
        private readonly AppConfig _appConfig;
        private InstallingViewModel _viewModel;
        public InstallingView(AppConfig appConfig, InstallingViewModel viewModel)
        {
            InitializeComponent();
            _appConfig = appConfig;
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        public void UpdateProgress(
            int progress,
            string status,
            string detail)
        {
            progress = Math.Clamp(progress, 0, 100);

            _viewModel.Progress = progress;
            _viewModel.Status = status;
            _viewModel.Detail = detail;
        }

    }
}
