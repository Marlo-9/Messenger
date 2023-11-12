using System.Windows;
using Messenger.Resources.ViewModel;
using Messenger.Resources.View;
using Messenger.Resources.Tools.Additional;


namespace Messenger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            
            var mainVm = new MainWindowVm();
            var window = new MainWindow() {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = mainVm
            };
            
            window.Closed += (_, _) => Logging.EndSession();
            
            window.ShowDialog();
        }
    }
}