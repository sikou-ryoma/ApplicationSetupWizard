using ApplicationSetupWizardLite.Conf;
using ApplicationSetupWizardLite.Context;
using ApplicationSetupWizardLite.Navigation;
using ApplicationSetupWizardLite.Paths;
using ApplicationSetupWizardLite.Services;
using ApplicationSetupWizardLite.Views;
using ApplicationSetupWizardLite.ViewModels;
using ApplicationSetupWizardLite.Commands;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Reflection;


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
        // private StepViewState stepViewState = new StepViewState();
        private StepViewState? currentStepState;
        private SetupContext setupCtx;
        private SetupResult setupResult = new SetupResult();

        // view models
        private InstallLocationViewModel installLocationViewModel;
        private InstallingViewModel installingViewModel;

        // services
        private ProcessService procService;
        private ShortcutService shortcutService = new ShortcutService();
        private ApplicationLaunchService launchService = new ApplicationLaunchService();
        private PayloadService payloadService = new PayloadService();

        // commands
        public AsyncRelayCommand NextCommand { get; }

        // strings
        private string? extractedPayloadPath;
        private string exePath = string.Empty;
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

            // データバインド
            DataContext = this;
            NextCommand = new AsyncRelayCommand(
                () => ExecuteNextAsync(),
                HandleCommandError,
                () => currentStepState?.CanGoNext ?? false);

            // xml読み込み
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(
                "ApplicationSetupWizardLite.Conf.config.xml");

            if (stream == null)
            {
                throw new FileNotFoundException(
                    "埋め込みリソース config.xml が見つかりません。");
            }

            xml = new XmlHelper(stream);

            // 設定とコンテクスト
            appConf = new AppConfig(xml);
            setupCtx = new SetupContext(appConf);
            procService = new ProcessService(xml, procCtx, setupCtx);

            // 埋め込みPayloadを一時フォルダへ展開
            extractedPayloadPath = payloadService.ExtractPayload();

            // 展開したPayloadの容量を取得
            setupCtx.PayloadSizeText =
                procService.FormatFileSize(
                    procService.GetPayloadSize(extractedPayloadPath));

            // 初期画面の表示
            GetStepMessages();
            ChangeContent(procCtx.currentStep);
            AppNameLbl1.Text = appConf.targetAppName + " v" + appConf.targetAppVersion;
            VersionLbl.Text = "Installer Version " + appConf.appVersion;
            setupCtx.InstallPath = paths.defaultInstallPath;

            // ViewModelの読み込み
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
                        NextButtonText = "インストール中",
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

            currentStepState = GetStepState(step);
            var state = currentStepState;

            ContentArea.Content = state.View;
            NextButton.Content = state.NextButtonText;
            BackButton.IsEnabled = state.CanGoBack;
            // NextButton.IsEnabled = state.CanGoNext;

            CancelButton.Visibility =
                state.ShowCancel ? Visibility.Visible : Visibility.Collapsed;
            BackButton.Visibility =
                state.ShowBack ? Visibility.Visible : Visibility.Collapsed;

            NextCommand.RaiseCanExecuteChanged();
        }


        // 以下コントロールのイベントとcommand処理


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
                    return;
                }
                CleanupTempFiles();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // 次へボタンの処理
        
        // 次へボタン押下時
        // ===============================
        private async Task ExecuteNextAsync()
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
            }
            
            MoveToNextStep();
        }

        private void HandleCommandError(Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"処理中にエラーが発生しました。\n\n{ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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

            try
            {
                // 展開済みPayloadのパスを確認
                if (string.IsNullOrWhiteSpace(extractedPayloadPath))
                {
                    throw new InvalidOperationException(
                        "展開済みPayloadのパスが設定されていません。");
                }

                // インストールを実行
                setupResult = await installingViewModel.StartInstallAsync(
                    extractedPayloadPath,
                    installPath);

                // 失敗・キャンセル時はショートカット作成へ進まない
                if (setupResult.Status != SetupStatus.Success)
                {
                    return;
                }

                // ショートカットを作成する
                if (installLocationViewModel.CreateShortcut)
                {
                    exePath = Path.Combine(
                        installPath,
                        appConf.executableName);

                    shortcutService.CreateDesktopShortcut(
                        appConf.targetAppName + "_v" + appConf.targetAppVersion,
                        exePath);
                }
            }
            finally
            {
                CleanupTempFiles();
                // 完了画面へ移動
                ChangeContent(procCtx.currentStep);
            }
        }

        // 展開した一時ファイルは処理に関わらずここで削除
        private void CleanupTempFiles()
        {
            try
            {
                payloadService.CleanupPayload(extractedPayloadPath);
                extractedPayloadPath = null;
            }
            catch (Exception ex)
            {
                // 後片付けの失敗でインストール結果を上書きしない
                System.Diagnostics.Debug.WriteLine(
                    $"一時ファイルの削除に失敗しました: {ex.Message}");
            }
        }


        // アプリケーション起動の必要がある場合に起動する
        // ===============================
        private void LaunchApplicationIfNeeded()
        {
            if (ContentArea.Content is CompleteView completeView &&
                completeView.viewModel.LaunchApplication)
            {
                try
                {
                    // launchService.LaunchTest(exePath);
                    launchService.Launch(exePath);
                }
                catch(FileNotFoundException ex)
                {
                    System.Windows.MessageBox.Show(
                        ex.Message,
                        "起動時エラー",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
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