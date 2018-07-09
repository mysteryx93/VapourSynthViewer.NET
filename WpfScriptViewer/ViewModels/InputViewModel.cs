using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmDialogs;
using GalaSoft.MvvmLight.CommandWpf;

namespace EmergenceGuardian.WpfScriptViewer {
    public interface IInputViewModel : IWorkspaceViewModel, IModalDialogViewModel {
        string Text { get; set; }
        string Value { get; set; }
        Func<string, bool> Validate { get; set; }
    }

    public class InputViewModel : WorkspaceViewModel, IInputViewModel {
        public bool? DialogResult { get; set; }
        private string text;
        private string value;
        public Func<string, bool> Validate { get; set; }
        private bool isValid;

        public InputViewModel() {
            DisplayName = "Input";
        }

        public string Text {
            get => text;
            set => Set<string>(() => Text, ref text, value);
        }
        
        public string Value {
            get => value;
            set {
                Set<string>(() => Value, ref this.value, value);
                isValid = Validate == null || Validate.Invoke(value);
                OKCommand.RaiseCanExecuteChanged();
            }
        }

        private RelayCommand okCommand;
        public RelayCommand OKCommand => this.InitCommand(ref okCommand, OnOK, CanOK);

        private bool CanOK() => isValid;
        private void OnOK() {
            DialogResult = true;
            if (CloseCommand.CanExecute(null))
                CloseCommand.Execute(null);
        }
    }

    public class ValueEventArgs : EventArgs {
        public string Value { get; set; }

        public ValueEventArgs() { }
        public ValueEventArgs(string value) {
            this.Value = value;
        }
    }
}
