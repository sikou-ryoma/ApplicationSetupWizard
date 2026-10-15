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


        // コンストラクタ
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
            await Task.Delay(2000); // UIの更新を待つために少し待機

            try
            {
                // Serviceから届く進捗情報を受け取る
                var progress = new Progress<InstallProgress>(p =>
                {
                    Progress = p.Percent;
                    Status = p.Status;
                    Detail = p.Detail;
                });

                // インストール処理を実行
                await _installService.InstallAsync(
                    installPath,
                    progress);
                
                // 正常終了
                Progress = 100;
                Status = "インストールが完了しました。";
                Detail = "すべての処理が正常に終了しました。";
                await Task.Delay(500); // UIの更新を待つために少し待機

                return new SetupResult()
                {
                    IsSuccess = true,
                    StatusText = "インストールが完了しました。",
                    DetailText = "すべての処理が正常に終了しました。"
                };
            }
            catch (Exception ex)
            {
                // エラー発生時
                Status = "インストールに失敗しました。";
                Detail = ex.Message;

                return new SetupResult()
                {
                    IsSuccess = false,
                    StatusText = "インストールに失敗しました。",
                    DetailText = ex.Message
                };
            }
            finally
            {
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
    }
}