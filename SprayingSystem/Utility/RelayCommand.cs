using System;
using System.Windows.Input;

namespace SprayingSystem.Utility
{
    public class RelayCommand : ICommand
    {
        private Action<object> _execute;
        private Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return false;

            // relay command - Never do this! But right now I don't have  better solution! :(
            //Refresh();

            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (_execute == null)
                return;

            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void Refresh()
        {
            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        public void Refresh2()
        {
            // Here we cannot use CanExecuteChanged to fire an event.
            // Is that because the way I defined my event?

            //if (CanExecuteChanged != null)
            //    CanExecuteChanged(this, new EventArgs());
        }
    }
}
