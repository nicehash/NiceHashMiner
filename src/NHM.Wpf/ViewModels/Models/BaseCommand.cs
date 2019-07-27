using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NHM.Wpf.ViewModels.Models
{
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
