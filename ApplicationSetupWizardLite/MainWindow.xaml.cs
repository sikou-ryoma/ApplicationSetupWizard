using ApplicationSetupWizardLite.Conf;
using ApplicationSetupWizardLite.Context;
using ApplicationSetupWizardLite.Navigation;
using ApplicationSetupWizardLite.Paths;
using ApplicationSetupWizardLite.Services;
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
        // configuration
        private AppConfig appConf;
        private XmlHelper xml;
        private PathManager paths =     
            new PathManager(Path.GetFullPath(AppContext.BaseDirectory));

        // context
        private ProcessContext procCtx = new ProcessContext();
        private StepViewState stepViewState = new StepViewState();
        private SetupContext setupCtx;
        private SetupResult setupResult = new SetupResult();

        // view models
        private InstallLocationViewModel installLocationViewModel;
        private InstallingViewModel installingViewModel;

        // services
        private ProcessService procService;
        private ShortcutService shortcutService = new ShortcutService();
        private ApplicationLaunchService launchService = new ApplicationLaunchService();

        // strings
        private readonly string[] stepTexts =
        {
            "ようこそ",
            "インストール先",
            "確認",
            "インストール",
            "完了"
        };

        private string exePath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            xml = new XmlHelper(paths.xmlFilePath);
            appConf = new AppConfig(xml);
            setupCtx = new SetupContext(appConf);
            procService = new ProcessService(xml, procCtx, setupCtx);

            GetStepMessages();
            ChangeContent(procCtx.currentStep);

            AppNameLbl1.Text = appConf.targetAppName + " v" + appConf.targetAppVersion;
            VersionLbl.Text = "Installer Version " + appConf.appVersion;
            setupCtx.InstallPath = paths.defaultInstallPath;

            installLocationViewModel = new InstallLocationViewModel(setupCtx);
            installingViewModel = new InstallingViewModel(procService);

        }

        // 基本的な画面表示

        // 左側ステップテキストのステータス取得
        // ===============================

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

        // 画面遷移時の状態設定の取得
        // ===============================

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
                        CanGoNext = true,
                        ShowCancel = true,
                        ShowBack = true
                    };

                case 1:
                    return new StepViewState
                    {
                        View = new InstallLocationView(setupCtx, installLocationViewModel),
                        NextButtonText = "次へ ＞",
                        BackButtonText = "＜ 戻る",
                        CanGoBack = true,
                        CanGoNext = true,
                        ShowCancel = true,
                        ShowBack = true
                    };

                case 2:
                    return new StepViewState
                    {
                        View = new ConfirmView(appConf, setupCtx),
                        NextButtonText = "インストール",
                        BackButtonText = "＜ 戻る",
                        CanGoBack = true,
                        CanGoNext = true,
                        ShowCancel = true,
                        ShowBack = true
                    };

                case 3:
                    return new StepViewState
                    {
                        View = new InstallingView(installingViewModel),
                        NextButtonText = "インストール",
                        BackButtonText = "＜ 戻る",
                        CanGoBack = false,
                        CanGoNext = false,
                        ShowCancel = true,
                        ShowBack = false
                    };

                case 4:
                    return new StepViewState
                    {
                        View = new CompleteView(setupResult),
                        NextButtonText = "完了",
                        CanGoBack = false,
                        CanGoNext = true,
                        ShowCancel = false,
                        ShowBack = false
                    };
            }
            throw new ArgumentOutOfRangeException(nameof(step));
        }

        // 画面遷移実行メソッド
        // ===============================

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
            NextButton.IsEnabled = state.CanGoNext;

            CancelButton.Visibility =
                state.ShowCancel ? Visibility.Visible : Visibility.Collapsed;
            BackButton.Visibility =
                state.ShowBack ? Visibility.Visible : Visibility.Collapsed;
        }


        // 以下コントロールのイベントハンドラと処理の呼び出し


        // キャンセル、閉じるボタンの処理
        // ===============================
        private bool _cancelRequested;

        private void Window_Closing(
            object sender,
            System.ComponentModel.CancelEventArgs e)
        {
            // インストール中の場合
            if (installingViewModel?.IsInstalling == true)
            {
                var result = System.Windows.MessageBox.Show(
                    "インストール中です。中止しますか？",
                    "確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                // ウィンドウはまだ閉じず、キャンセルを要求する
                _cancelRequested = true;
                e.Cancel = true;

                installingViewModel.CancelInstall();
                return;
            }

            // セットアップが完了していない場合
            if (!procCtx.IsCompleted)
            {
                var result = System.Windows.MessageBox.Show(
                    "セットアップを中止しますか？",
                    "確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

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

        // 次へボタンの処理

        // 押下時の挙動
        // ===============================
        private async void NextButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (procCtx.IsCompleted)
            {
                LaunchApplicationIfNeeded();
                Close();
                return;
            }

            if (procCtx.currentStep == 2)
            {
                await ExecuteInstallationAsync();
                // return;
            }

            MoveToNextStep();
        }

        // 基本処理
        // ===============================
        private void MoveToNextStep()
        {
            procService.NextProcess();

            GetStepMessages();
            ChangeContent(procCtx.currentStep);
        }

        // インストール処理の実行
        // ===============================
        private async Task ExecuteInstallationAsync()
        {
            // インストール画面へ移動
            MoveToNextStep();

            string installPath =
                setupCtx.GetFullInstallPath(
                    setupCtx.InstallPath);

            // インストールを実行
            setupResult =
                await installingViewModel.StartInstallAsync(
                    installPath);

            // 失敗時はショートカット作成へ進まない
            if (setupResult.Status != SetupStatus.Success)
            {
                ChangeContent(procCtx.currentStep);
                return;
            }

            // ショートカットを作成する
            if (installLocationViewModel.CreateShortcut)
            {
                exePath = Path.Combine(
                    setupCtx.GetFullInstallPath(setupCtx.InstallPath),
                    // appConf.executableName);
                    "test.txt");

                shortcutService.CreateDesktopShortcut(
                    appConf.targetAppName + "_v" + appConf.targetAppVersion,
                    exePath);
            }

            ChangeContent(procCtx.currentStep);
        }

        // アプリケーション起動の必要がある場合に起動する
        // ===============================
        private void LaunchApplicationIfNeeded()
        {
            if (ContentArea.Content is CompleteView completeView)
            {
                if (completeView.viewModel.LaunchApplication)
                {
                    launchService.LaunchTest(exePath);
                    // launchService.Launch(exePath);
                }
            }
        }

        // 戻るボタンの処理
        // ===============================
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            procService.PreviousProcess();
            GetStepMessages();
            ChangeContent(procCtx.currentStep);
        }
    }
}