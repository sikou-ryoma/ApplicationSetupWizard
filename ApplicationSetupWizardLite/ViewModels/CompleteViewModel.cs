
using ApplicationSetupWizardLite.Context;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ApplicationSetupWizardLite.ViewModels
{
    public class CompleteViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isSuccess;
        private string _description = "";
        private string _resultTitle = "";
        private string _resultDetail = "";
        private bool _canLaunchApplication;
        public string ResultSymbol => IsSuccess ? "✓" : "!";
        public string ResultBackground =>
            IsSuccess ? "#4CAF50" : "#D9534F";
        public bool ShowLaunchOption => IsSuccess;


        public bool IsSuccess
        {
            get => _isSuccess;
            private set
            {
                _isSuccess = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => _description;
            private set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public string ResultTitle
        {
            get => _resultTitle;
            private set
            {
                _resultTitle = value;
                OnPropertyChanged();
            }
        }

        public string ResultDetail
        {
            get => _resultDetail;
            private set
            {
                _resultDetail = value;
                OnPropertyChanged();
            }
        }

        public bool CanLaunchApplication
        {
            get => _canLaunchApplication;
            private set
            {
                _canLaunchApplication = value;
                OnPropertyChanged();
            }
        }

        public CompleteViewModel(SetupResult result)
        {
            IsSuccess = result.IsSuccess;
            Description = result.IsSuccess
                ? "アプリケーションのインストールが正常に完了しました。"
                : "アプリケーションのインストール中に問題が発生しました。";

            ResultTitle = result.StatusText;
            ResultDetail = result.DetailText;

            CanLaunchApplication = result.IsSuccess;
        }

        private void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
