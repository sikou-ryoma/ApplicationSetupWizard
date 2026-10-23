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
using Microsoft.Extensions.Logging;


namespace ApplicationSetupWizardLite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // configuration
        private readonly AppConfig _appConfig;
        private readonly XmlHelper _xml;
        private readonly PathManager _paths =     
            new PathManager(Path.GetFullPath(AppContext.BaseDirectory));

        // context
        private readonly ProcessContext _processContext = new ProcessContext();
        private StepViewState? _currentStepState;
        private SetupContext _setupContext;
        private SetupResult _setupResult = new SetupResult();

        // view models
        private InstallLocationViewModel _installLocationViewModel;
        private InstallingViewModel _installingViewModel;

        // services
        private ProcessService _processService;
        private ShortcutService _shortcutService = new ShortcutService();
        private readonly ApplicationLaunchService _launchService =
            new ApplicationLaunchService();
        private readonly PayloadService _payloadService = new PayloadService();

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
                () => _currentStepState?.CanGoNext ?? false);

            // xml読み込み
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(
                "ApplicationSetupWizardLite.Conf.config.xml");

            if (stream == null)
            {
                throw new FileNotFoundException(
                    "埋め込みリソース config.xml が見つかりません。");
            }

            _xml = new XmlHelper(stream);

            // 設定とコンテクスト
            _appConfig = new AppConfig(_xml);
            _setupContext = new SetupContext(_appConfig);
            _processService = new ProcessService(
                _xml,
                _processContext, 
                _setupContext,
                App.LoggerFactory.CreateLogger<ProcessService>());

            // 初期画面の表示
            GetStepMessages();
            ChangeContent(_processContext.CurrentStep);
            AppNameLbl1.Text = _appConfig.TargetAppName + " v" + _appConfig.TargetAppVersion;
            VersionLbl.Text = "Installer Version " + _appConfig.AppVersion;
            _setupContext.InstallPath = _paths.DefaultInstallPath;

            // ViewModelの読み込み
            _installLocationViewModel = new InstallLocationViewModel(_setupContext);
            _installingViewModel = new InstallingViewModel(_processService);
        }

        // Payload全体の容量数をロード時に取得
        // ===============================
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadPayloadSizeAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Payloadの読み込みに失敗しました。\n\n{ex.Message}",
                    "初期化エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
        }

        private async Task LoadPayloadSizeAsync()
        {
            string? payloadPath = null;

            try
            {
                var result = await Task.Run(() =>
                {
                    string path = _payloadService.ExtractPayload();

                    try
                    {
                        long size = _processService.GetPayloadSize(path);
                        return (payloadPath: path, size);
                    }
                    catch
                    {
                        // サイズ取得に失敗した場合も、
                        // 展開したPayloadを削除する
                        _payloadService.CleanupPayload(path);
                        throw;
                    }
                });

                payloadPath = result.payloadPath;
                _setupContext.PayloadSizeText =
                    _processService.FormatFileSize(result.size);

                extractedPayloadPath = payloadPath;
            }
            catch
            {
                // UI側でサイズ表示などの処理に失敗した場合も
                // 展開済みPayloadを削除する
                _payloadService.CleanupPayload(payloadPath);
                throw;
            }
        }

        // 基本的な画面表示

        // 左側ステップテキストのステータス取得
        // ===============================

        private string GetStepStatus(int step)
        {
            if (step == _processContext.CurrentStep)
                return "▶  " + stepTexts[step];
            if (step < _processContext.CurrentStep)
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
                        View = new WelcomeView(_appConfig),
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
                        View = new InstallLocationView(_setupContext, _installLocationViewModel),
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
                        View = new ConfirmView(_appConfig, _setupContext),
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
                        View = new InstallingView(_installingViewModel),
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
                        View = new CompleteView(_setupResult),
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
                _installLocationViewModel.UpdateContext();
            }

            _currentStepState = GetStepState(step);
            var state = _currentStepState;

            ContentArea.Content = state.View;
            NextButton.Content = state.NextButtonText;
            BackButton.IsEnabled = state.CanGoBack;

            CancelButton.Visibility =
                state.ShowCancel ? Visibility.Visible : Visibility.Collapsed;
            BackButton.Visibility =
                state.ShowBack ? Visibility.Visible : Visibility.Collapsed;

            NextCommand.RaiseCanExecuteChanged();
        }


        // 以下コントロールのイベントとcommand処理


        // キャンセル、閉じるボタンの処理
        // ===============================

        private void Window_Closing(
            object sender,
            System.ComponentModel.CancelEventArgs e)
        {
            // インストール中の場合
            if (_installingViewModel?.IsInstalling == true)
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
                e.Cancel = true;

                _installingViewModel.CancelInstall();
                return;
            }

            // セットアップが完了していない場合
            if (!_processContext.IsCompleted)
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
            if (_processContext.IsCompleted)
            {
                LaunchApplicationIfNeeded();
                Close();
                return;
            }

            if (_processContext.CurrentStep == 2)
            {
                await ExecuteInstallationAsync();
            }

            // 基本的にキャンセル・失敗時でも完了画面に遷移する
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
            _processService.NextProcess();

            GetStepMessages();
            ChangeContent(_processContext.CurrentStep);
        }

        // インストール処理の実行
        // ===============================
        private async Task ExecuteInstallationAsync()
        {
            // インストール画面へ移動
            MoveToNextStep();

            string installPath =
                _setupContext.GetFullInstallPath(
                    _setupContext.InstallPath);

            try
            {
                // 展開済みPayloadのパスを確認
                if (string.IsNullOrWhiteSpace(extractedPayloadPath))
                {
                    throw new InvalidOperationException(
                        "展開済みPayloadのパスが設定されていません。");
                }

                // インストールを実行
                _setupResult = await _installingViewModel.StartInstallAsync(
                    extractedPayloadPath,
                    installPath);

                exePath = Path.Combine(
                    _setupContext.GetFullInstallPath(_setupContext.InstallPath),
                    _appConfig.ExecutableName);

                // 失敗・キャンセル時はショートカット作成へ進まない
                if (_setupResult.Status != SetupStatus.Success)
                {
                    return;
                }

                // ショートカットを作成する

                if (_installLocationViewModel.CreateShortcut)
                {
                    try
                    {
                        _shortcutService.CreateDesktopShortcut(
                            _appConfig.TargetAppName + "_v" + _appConfig.TargetAppVersion,
                            exePath);

                        _setupResult.ShortcutStatus = ShortcutStatus.Created;
                        _setupResult.ShortcutDetailText =
                            "ショートカットを作成しました。";
                    }
                    catch (Exception ex)
                    {
                        _setupResult.ShortcutStatus = ShortcutStatus.Failed;
                        _setupResult.ShortcutDetailText =
                            "ショートカットを作成できませんでした。";

                        // 詳細な原因はデバッグ時に確認できるようにする
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                }
            }
            finally
            {
                CleanupTempFiles();
            }
        }

        // 展開した一時ファイルは処理に関わらずここで削除
        private void CleanupTempFiles()
        {
            try
            {
                _payloadService.CleanupPayload(extractedPayloadPath);
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
                completeView.ViewModel.LaunchApplication)
            {
                try
                {
                    // launchService.LaunchTest(exePath);
                    _launchService.Launch(exePath);
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
            _processService.PreviousProcess();
            GetStepMessages();
            ChangeContent(_processContext.CurrentStep);
        }
    }
}