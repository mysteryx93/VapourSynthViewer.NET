using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace EmergenceGuardian.WpfScriptViewer {
    public static class CommandHelper {
        /// <summary>
        /// Creates a new DelegateCommand the first time it is called and then re-use that command.
        /// </summary>
        /// <param name="cmd">The reference of the command object to initialize.</param>
        /// <param name="execute">The method this command will execute.</param>
        /// <param name="canExecute">The method returning whether command can execute.</param>
        /// <returns>The initialized command object.</returns>
        public static RelayCommand InitCommand(this ObservableObject obj, ref RelayCommand cmd, Action execute, Func<bool> canExecute) {
            if (cmd == null)
                cmd = new RelayCommand(execute, canExecute);
            return cmd;
        }
    }
}
