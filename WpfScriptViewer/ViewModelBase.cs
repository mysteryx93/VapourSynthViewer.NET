using EmergenceGuardian.MediaPlayerUI;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace EmergenceGuardian.WpfScriptViewer {

    /// <summary>
    /// This is the abstract base class for any object that provides property change notifications.  
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged {
        protected ViewModelBase() { }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void RaisePropertyChanged(string propertyName) {
            this.VerifyPropertyName(propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName) {
            // If you raise PropertyChanged and do not specify a property name,
            // all properties on the object are considered to be changed by the binding system.
            if (String.IsNullOrEmpty(propertyName))
                return;

            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null) {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new ArgumentException(msg);
                else
                    Debug.Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new DelegateCommand the first time it is called and then re-use that command.
        /// </summary>
        /// <param name="cmd">The reference of the command object to initialize.</param>
        /// <param name="execute">The method this command will execute.</param>
        /// <param name="canExecute">The method returning whether command can execute.</param>
        /// <returns>The initialized command object.</returns>
        protected ICommand InitCommand(ref ICommand cmd, Action execute, Func<bool> canExecute) {
            if (cmd == null)
                cmd = new DelegateCommand(execute, canExecute);
            return cmd;
        }
    }
}
