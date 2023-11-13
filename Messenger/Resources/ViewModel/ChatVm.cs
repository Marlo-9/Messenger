using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Messenger.Resources.Tools.Connection;
using Messenger.Resources.Tools.Data;
using Wpf.Ui.Controls;

namespace Messenger.Resources.ViewModel;

public partial class ChatVm : ObservableObject
{
    [ObservableProperty] private UserInfo _user;
    [ObservableProperty] private string _messageText;
    [ObservableProperty] private ObservableCollection<Message> _messages = new ObservableCollection<Message>();

    private SettingsVm _settingsVm;

    public ChatVm() {}
    
    public ChatVm(UserInfo userRecipient, SettingsVm settingsVm)
    {
        User = userRecipient;
        _settingsVm = settingsVm;
        _settingsVm.NewMessage += message =>
        {
            if (!_settingsVm.User.Id.Equals(message.Info.RecipientId)) return;

            message.ControlAppearance = ControlAppearance.Secondary;
            
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() => Messages.Add(message))
            );
        };
    }

    [RelayCommand]
    private void SendMessage(object obj)
    {
        TextBox? senderTextBox = obj as TextBox;
        Message message = new Message(senderTextBox!.Text, User.Id);
        
        _settingsVm.SendMessage(message);
        message.ControlAppearance = ControlAppearance.Primary;
        Messages.Add(message);
        senderTextBox.Text = "";
    }

}