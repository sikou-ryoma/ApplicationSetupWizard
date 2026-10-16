using ApplicationSetupWizardLite.Context;
using ApplicationSetupWizardLite.Service;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ApplicationSetupWizardLite.ViewModels
{
    public class InstallingViewModel : INotifyPropertyChanged
    {
        private readonly ProcessService _installService;
        public event PropertyChangedEventHandler? PropertyChanged;
        private CancellationTokenSource? _installCts;

        
        public InstallingViewModel(ProcessService installService)
        {
            _installService = installService;
        }


        // 進捗率
        private int _progress;

        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }


        // 現在の処理
        private string _status = "インストールの準備をしています...";

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }


        // 詳細情報
        private string _detail = "インストールを開始しています...";

        public string Detail
        {
            get => _detail;
            set => SetProperty(ref _detail, value);
        }


        // インストール中かどうか
        private bool _isInstalling;

        public bool IsInstalling
        {
            get => _isInstalling;
            set => SetProperty(ref _isInstalling, value);
        }


        // インストール処理の開始
        public async Task<SetupResult> StartInstallAsync(string installPath)
        {
            IsInstalling = true;

            Progress = 0;
            Status = "インストールを開始しています...";
            Detail = "準備中です。";
            _installCts = new CancellationTokenSource();
            await Task.Delay(2000); // UIの更新を待つために少し待機

            try
            {
                var progress = new Progress<InstallProgress>(p =>
                {
                    Progress = p.Percent;
                    Status = p.Status;
                    Detail = p.Detail;
                });

                await _installService.InstallAsync(
                    installPath,
                    progress,
                    _installCts.Token);

                Progress = 100;
                Status = "インストールが完了しました。";
                Detail = "すべての処理が正常に終了しました。";
                await Task.Delay(1000);

                return new SetupResult
                {
                    Status = SetupStatus.Success,
                    StatusText = Status,
                    DetailText = Detail
                };
            }
            catch (OperationCanceledException)
            {
                Status = "インストールがキャンセルされました。";
                Detail = "インストール処理を中断しました。";
                await Task.Delay(1000);

                return new SetupResult
                {
                    Status = SetupStatus.Cancelled,
                    StatusText = Status,
                    DetailText = Detail
                };
            }
            catch (Exception ex)
            {
                Status = "インストールに失敗しました。";
                await Task.Delay(1000);
                Detail = ex.Message;

                return new SetupResult
                {
                    Status = SetupStatus.Failed,
                    StatusText = Status,
                    DetailText = Detail
                };
            }
            finally
            {
                _installCts?.Dispose();
                _installCts = null;
                IsInstalling = false;
            }
        }


        // プロパティ変更通知の共通処理
        protected bool SetProperty<T>(
            ref T field,
            T value,
            [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));

            return true;
        }

        // インストールのキャンセル要求
        public void CancelInstall()
        {
            if (IsInstalling)
            {
                _installCts?.Cancel();
            }
        }
    }
}