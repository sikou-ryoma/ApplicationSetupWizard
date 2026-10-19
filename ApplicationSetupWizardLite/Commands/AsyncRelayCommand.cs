
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ApplicationSetupWizardLite.Commands
{
    public sealed class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Action<Exception> _onError;
        private readonly Func<bool> _canExecute;

        private bool _isExecuting;

        public AsyncRelayCommand(
            Func<Task> execute,
            Action<Exception> onError,
            Func<bool>? canExecute = null)
        {
            _execute = execute;
            _onError = onError;
            _canExecute = canExecute ?? (() => true);
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && _canExecute();
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            RaiseCanExecuteChanged();

            try
            {
                await _execute();
            }
            catch (Exception ex)
            {
                _onError(ex);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }
    }
}
