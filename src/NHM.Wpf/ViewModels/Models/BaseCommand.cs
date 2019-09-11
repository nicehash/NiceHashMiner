using System;
using System.Windows.Input;

namespace NHM.Wpf.ViewModels.Models
{
    /// <summary>
    /// Basic implementation of ICommand that takes an Action with optional object parameter.
    /// </summary>
    internal class BaseCommand : ICommand
    {
        private readonly Action<object> _action;

        public BaseCommand(Action<object> action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}
