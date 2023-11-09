using CommunityToolkit.Mvvm.ComponentModel;
using Messenger.Resources.Data;

namespace Messenger.Resources.ViewModel;

public partial class ChatVM : ObservableObject
{
    [ObservableProperty] private UserInfo _user;
    [ObservableProperty] private string _messageText;

    public ChatVM() {}
    
    public ChatVM(UserInfo user)
    {
        User = user;
    }
}