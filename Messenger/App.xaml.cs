using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Messenger.Resources.ViewModel;
using Messenger.Resources.View;

namespace Messenger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            
            MainWindowVm mainVm = new MainWindowVm();
            MainWindow window = new MainWindow() {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = mainVm
            };
            
            window.ShowDialog();
        }
    }
}