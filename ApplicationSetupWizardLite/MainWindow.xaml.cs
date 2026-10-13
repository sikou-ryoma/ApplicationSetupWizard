using ApplicationSetupWizardLite.Conf;
using ApplicationSetupWizardLite.Context;
using ApplicationSetupWizardLite.Navigation;
using ApplicationSetupWizardLite.Paths;
using ApplicationSetupWizardLite.Service;
using ApplicationSetupWizardLite.Views;
using ApplicationSetupWizardLite.ViewModels;
using System.IO;
using System.Windows;


namespace ApplicationSetupWizardLite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PathManager paths =     
            new PathManager(Path.GetFullPath(AppContext.BaseDirectory));
        private XmlHelper xml;
        private ProcessContext procCtx = new ProcessContext();
        private SetupContext setupCtx;
        private ProcessService procService;
        private AppConfig appConf;
        private StepViewState stepViewState = new StepViewState();
        private InstallLocationViewModel installLocationViewModel;
        private readonly string[] stepTexts =
        {
            "ようこそ",
            "インストール先",
            "確認",
            "インストール",
            "完了"
        };

        public MainWindow()
        {
            InitializeComponent();
            xml = new XmlHelper(paths.xmlFilePath);
            appConf = new AppConfig(xml);
            setupCtx = new SetupContext(appConf);
            procService = new ProcessService(xml, procCtx, setupCtx);
            GetStepMessages();
            ChangeContent(procCtx.currentStep);
            AppNameLbl1.Text = appConf.appName;
            VersionLbl.Text = "Version " + appConf.appVersion;

            setupCtx.InstallPath = paths.defaultInstallPath;
            installLocationViewModel = new InstallLocationViewModel(setupCtx);

        }

        private string GetStepStatus(int step)
        {
            if (step == procCtx.currentStep)
                return "▶  " + stepTexts[step];
            if (step < procCtx.currentStep)
                return "✓  " + stepTexts[step];
            return "〇  " + stepTexts[step];
        }

        private void GetStepMessages()
        {
            Step1.Text = GetStepStatus(0);
            Step2.Text = GetStepStatus(1);
            Step3.Text = GetStepStatus(2);
            Step4.Text = GetStepStatus(3);
            Step5.Text = GetStepStatus(4);
        }

        private StepViewState GetStepState(int step)
        {
            switch (step)
            {
                case 0:
                    return new StepViewState
                    {
                        View = new WelcomeView(appConf),
                        NextButtonText = "次へ ＞",
                        BackButtonText = "＜ 戻る",
                        CanGoBack = false,
                        ShowCancel = true,
                        ShowBack = true
                    };

                case 1:
                    return new StepViewState
                    {
                        View = new InstallLocationView(appConf, xml, paths, procCtx, setupCtx, installLocationViewModel),
                        NextButtonText = "次へ ＞",
                        BackButtonText = "＜ 戻る",
                        CanGoBack = true,
                        ShowCancel = true,
                        ShowBack = true
                    };

                case 2:
                    return new StepViewState
                    {
                        View = new ConfirmView(appConf, xml, paths, procCtx, setupCtx),
                        NextButtonText = "インストール",
                        BackButtonText = "＜ 戻る",
                        CanGoBack = true,
                        ShowCancel = true,
                        ShowBack = true
                    };

                case 3:
                    return new StepViewState
                    {
                        View = new InstallingView(appConf),
                        NextButtonText = "インストール",
                        BackButtonText = "＜ 戻る",
                        CanGoBack = false,
                        ShowCancel = true,
                        ShowBack = false
                    };

                case 4:
                    return new StepViewState
                    {
                        View = new CompleteView(appConf),
                        NextButtonText = "完了",
                        CanGoBack = false,
                        ShowCancel = false,
                        ShowBack = false
                    };
            }
            throw new ArgumentOutOfRangeException(nameof(step));
        }

        private void ChangeContent(int step)
        {
            if (step == 1 || step == 2)
            {
                installLocationViewModel.UpdateContext();
            }

            var state = GetStepState(step);

            ContentArea.Content = state.View;
            NextButton.Content = state.NextButtonText;
            BackButton.IsEnabled = state.CanGoBack;
            CancelButton.Visibility =
                state.ShowCancel ? Visibility.Visible : Visibility.Collapsed;
            BackButton.Visibility =
                state.ShowBack ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (procCtx.currentStep == 3)
            {
                var result = System.Windows.MessageBox.Show(
                    "インストール中です。中止すると不完全な状態で終了します。\n本当に中止しますか？",
                    "確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
            else
                if (!procCtx.IsCompleted)
            {
                var result = System.Windows.MessageBox.Show(
                    "セットアップを中止しますか？",
                    "確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (procCtx.IsCompleted)
            {
                this.Close();
                return;
            }

            procService.NextProcess();
            GetStepMessages();
            ChangeContent(procCtx.currentStep);

            if(procCtx.currentStep == 3)
            {
                procService.NextProcess();

                // procService.Install(setupCtx.InstallPath);
                System.Windows.MessageBox.Show(
                    "インストールが完了しました。", 
                    "テスト用メッセージ", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information
                );

                GetStepMessages();
                ChangeContent(procCtx.currentStep);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            procService.PreviousProcess();
            GetStepMessages();
            ChangeContent(procCtx.currentStep);
        }

    }
}