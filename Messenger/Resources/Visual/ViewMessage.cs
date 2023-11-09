using System;
using System.Threading.Tasks;
using Wpf.Ui.Controls;

namespace Messenger.Resources.Visual;

public class ViewMessage
{
    public static ControlAppearance ErrorAppearance { get; set; }  = ControlAppearance.Danger;
    public static SymbolRegular ErrorIcon { get; set; } = SymbolRegular.ErrorCircle24;
    public static float ErrorShowSecond { get; set; } = 4;
    
    public static async Task ShowError(SnackbarPresenter presenter, string title, string message)
    {
        Snackbar snackbar = new Snackbar(presenter);
        
        snackbar.Title = title;
        snackbar.Content = message;
        snackbar.Timeout = TimeSpan.FromMilliseconds(ErrorShowSecond * 1000);
        snackbar.Appearance = ErrorAppearance;
        snackbar.Icon = new SymbolIcon(ErrorIcon);
        
        await snackbar.ShowAsync();
    }
}