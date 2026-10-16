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
        public CompleteView(AppConfig appConfig, SetupResult result)
        {
            InitializeComponent();

            DataContext = new CompleteViewModel(result);
        }
    }
}
