using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmDialogs;

namespace EmergenceGuardian.WpfScriptViewer {
    public static class InputHelper {
        public static object RequestInput<T>(INotifyPropertyChanged parent, IDialogService dialogService, string displayName, string text, T value, Func<T, bool> validate) {
            IInputViewModel InputForm = ViewModelLocator.Instance.Input;
            // How to manage class instance? For now it's always using the same ViewModel isntance so reset it.
            (InputForm as InputViewModel).DialogResult = null;
            InputForm.DisplayName = displayName;
            InputForm.Text = text;
            InputForm.Value = value.ToString();
            InputForm.Validate = new Func<string, bool>((e) => {
                try {
                    T Val = ChangeType<T>(e, null);
                    return validate(Val);
                } catch {
                    return false;
                }
            });
            if (dialogService.ShowDialog(parent, InputForm) == true) {
                try {
                    return ChangeType<T>(InputForm.Value, null);
                } catch {
                    return null;
                }
            } else
                return null;
        }

        public static T ChangeType<T>(object value) {
            return ChangeType<T>(value, null);
        }

        public static T ChangeType<T>(object value, CultureInfo cultureInfo) {
            var toType = typeof(T);

            if (value == null) return default(T);

            if (value is string) {
                if (toType == typeof(Guid)) {
                    return ChangeType<T>(new Guid(Convert.ToString(value, cultureInfo)), cultureInfo);
                }
                if ((string)value == string.Empty && toType != typeof(string)) {
                    return ChangeType<T>(null, cultureInfo);
                }
            } else {
                if (typeof(T) == typeof(string)) {
                    return ChangeType<T>(Convert.ToString(value, cultureInfo), cultureInfo);
                }
            }

            if (toType.IsGenericType &&
                toType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                toType = Nullable.GetUnderlyingType(toType); ;
            }

            bool canConvert = toType is IConvertible || (toType.IsValueType && !toType.IsEnum);
            if (canConvert) {
                return (T)Convert.ChangeType(value, toType, cultureInfo);
            }
            return (T)value;
        }
    }
}
