using ApplicationSetupWizardLite.Conf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ApplicationSetupWizardLite.Views
{
    /// <summary>
    /// WelcomeView.xaml の相互作用ロジック
    /// </summary>
    public partial class WelcomeView : System.Windows.Controls.UserControl
    {
       private readonly AppConfig _appConf;

        public WelcomeView(AppConfig appConf)
        {
            InitializeComponent();
            _appConf = appConf;

            TitleTextBlock.Text = $"{_appConf.targetAppName} v{ _appConf.targetAppVersion} セットアップ";
            ContentTextBlock.Text = $"{_appConf.targetAppName} をコンピューターにインストールします。";
        }
    }
}
