using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Common
{
    public class Command : ICommand
    {
        public delegate bool CanExcuteDelegate(object parameter);
        public delegate void ExecuteDelegate(object parameter);

        private CanExcuteDelegate _CanExecute;
        private ExecuteDelegate _Execute;

        public Command(CanExcuteDelegate can_exec, ExecuteDelegate exec)
        {
            _CanExecute = can_exec;
            _Execute = exec;
        }
    
        public bool CanExecute(object parameter)
        {
            if (_CanExecute == null) return false;

            return _CanExecute.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (_Execute == null) return;

            _Execute.Invoke(parameter);
        }

        public void UpdateCanExecuteState()
        {
            this.CanExecuteChanged(this, new EventArgs());
        }
    }
}
