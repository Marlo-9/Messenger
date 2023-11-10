using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Messenger.Resources.Connection;
using Messenger.Resources.Data;
using Wpf.Ui.Controls;

namespace Messenger.Resources.ViewModel;

public partial class ChatVM : ObservableObject
{
    [ObservableProperty] private UserInfo _user;
    [ObservableProperty] private string _messageText;
    [ObservableProperty] private ObservableCollection<Message> _messages = new ObservableCollection<Message>();

    private SettingsVm _settingsVm;

    public ChatVM() {}
    
    public ChatVM(UserInfo userRecipient, SettingsVm settingsVm)
    {
        User = userRecipient;
        _settingsVm = settingsVm;
    }

    [RelayCommand]
    private void SendMessage(object obj)
    {
        TextBox senderTextBox = obj as TextBox;
        
        _settingsVm.SendMessage(new Message(_user.Id, senderTextBox.Text));

        senderTextBox.Text = "";
    }

}