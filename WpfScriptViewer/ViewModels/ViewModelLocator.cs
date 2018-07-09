/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:EmergenceGuardian.WpfScriptViewer"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using CommonServiceLocator;
using EmergenceGuardian.WpfExtensions;
using System.Windows;
using MvvmDialogs;
using System;
using System.ComponentModel;

namespace EmergenceGuardian.WpfScriptViewer {
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator {
        private static ViewModelLocator instance;
        public static ViewModelLocator Instance => instance ?? (instance = (ViewModelLocator)Application.Current.FindResource("Locator"));

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator() {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Reset();
            bool IsDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());

            // Register services.
            SimpleIoc.Default.Register<IDialogService>(() => new DialogService(null, new DialogTypeLocator(), null));
            SimpleIoc.Default.Register<IEnvironmentService, EnvironmentService>();

            // Register ViewModels.
            SimpleIoc.Default.Register<MainViewModel>();
            if (IsDesignMode)
                SimpleIoc.Default.Register<IHelpViewModel, DesignHelpViewModel>();
            else
                SimpleIoc.Default.Register<IHelpViewModel, HelpViewModel>();
            SimpleIoc.Default.Register<IInputViewModel, InputViewModel>();
            SimpleIoc.Default.Register<IEditorViewModel, EditorViewModel>();
            SimpleIoc.Default.Register<IViewerViewModel, ViewerViewModel>();
        }

        public IDialogService DialogService => ServiceLocator.Current.GetInstance<IDialogService>();
        public IEnvironmentService EnvironmentService => ServiceLocator.Current.GetInstance<IEnvironmentService>();
        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public IHelpViewModel Help => ServiceLocator.Current.GetInstance<IHelpViewModel>();
        public IInputViewModel Input => SimpleIoc.Default.GetInstance<IInputViewModel>();
        public IEditorViewModel GetEditor() => SimpleIoc.Default.GetInstanceWithoutCaching<IEditorViewModel>();
        public IViewerViewModel GetViewer() => SimpleIoc.Default.GetInstanceWithoutCaching<IViewerViewModel>();

        public static void Cleanup() => SimpleIoc.Default.Reset();

        public class DialogTypeLocator : MvvmDialogs.DialogTypeLocators.IDialogTypeLocator {
            public Type Locate(INotifyPropertyChanged viewModel) {
                if (viewModel is MainViewModel)
                    return typeof(MainView);
                else if (viewModel is IHelpViewModel)
                    return typeof(HelpView);
                else if (viewModel is IInputViewModel)
                    return typeof(InputView);
                else
                    return null;
            }
        }
    }
}