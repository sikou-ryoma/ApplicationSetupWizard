
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
        private SetupStatus _status;
        public string ResultSymbol => _status switch
        {
            SetupStatus.Success => "✓",
            SetupStatus.Cancelled => "Ⅱ",
            _ => "!"
        };

        public string ResultBackground => _status switch
        {
            SetupStatus.Success => "#4CAF50",
            SetupStatus.Cancelled => "#E0A020",
            _ => "#D9534F"
        };

        public bool ShowLaunchOption => _status == SetupStatus.Success;

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
            _status = result.Status;
            IsSuccess = result.IsSuccess;

            Description = result.Status switch
            {
                SetupStatus.Success =>
                    "アプリケーションのインストールが正常に完了しました。",

                SetupStatus.Cancelled =>
                    "インストールはキャンセルされました。",

                _ =>
                    "アプリケーションのインストール中に問題が発生しました。"
            };

            ResultTitle = result.StatusText;
            ResultDetail = result.DetailText;

            CanLaunchApplication = result.IsSuccess;

            OnPropertyChanged(nameof(ResultSymbol));
            OnPropertyChanged(nameof(ResultBackground));
            OnPropertyChanged(nameof(ShowLaunchOption));
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
