using ApplicationSetupWizardLite.Conf;
using ApplicationSetupWizardLite.Paths;
using ApplicationSetupWizardLite.Context;
using ApplicationSetupWizardLite.ViewModels;
using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace ApplicationSetupWizardLite.Views
{
    /// <summary>
    /// InstallLocationView.xaml の相互作用ロジック
    /// </summary>
    public partial class InstallLocationView : UserControl
    {
        private SetupContext _setupCtx;
        private InstallLocationViewModel _viewModel;

        public InstallLocationView(
            SetupContext setupCtx,
            InstallLocationViewModel viewModel)
        {
            InitializeComponent();
            _setupCtx = setupCtx;
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.InstallPath = _setupCtx.InstallPath;
            _viewModel.CreateShortcut = _setupCtx.CreateShortcut;
            _viewModel.FullInstallPath = _setupCtx.GetFullInstallPath(_viewModel.InstallPath);
        }

        public void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _viewModel.InstallPath = dialog.SelectedPath;
                InstallPathTextBox.Text = _viewModel.InstallPath;

                _viewModel.FullInstallPath = _setupCtx.GetFullInstallPath(_viewModel.InstallPath);
                FolderStructureText.Text = _viewModel.FullInstallPath;
            }
        }
    }
}
