using Calculatrice.Model;
using System;
using System.Windows.Input;

namespace Calculatrice.ViewModel
{
    public class NumberCommand : ICommand
    {
        public NumberCommand(MainWindowVM vm)
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
            return vm.CanExecuteNumberCommand(parameter as string);
        }

        public void Execute(object parameter)
        {
            vm.ExecuteNumberCommand(parameter as string);
        }
    }
}
