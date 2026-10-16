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
        public CompleteViewModel viewModel { get; }

        public CompleteView(SetupResult result)
        {
            InitializeComponent();

            viewModel = new CompleteViewModel(result);
            DataContext = viewModel;
        }
    }
}
