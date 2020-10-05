using Calculatrice.Model;
using System;
using System.Windows.Input;

namespace Calculatrice.ViewModel
{
    public class OtherCommand : ICommand
    {
        public OtherCommand(MainWindowVM vm)
        {
            this.vm = vm;
        }

        private readonly MainWindowVM vm;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return vm.CanExecuteOtherCommand(parameter as string);
        }

        public void Execute(object parameter)
        {
            vm.ExecuteOtherCommand(parameter as string);
        }
    }
}
