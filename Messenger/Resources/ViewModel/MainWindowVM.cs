using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Messenger.Resources.Data;
using Messenger.Resources.View;

namespace Messenger.Resources.ViewModel;

public partial class MainWindowVm : ObservableObject
{
    [ObservableProperty] private string _title = "Мессенджер";
    [ObservableProperty] private Page _currentPage;
    
    [ObservableProperty] private ObservableCollection<UserInfo> _onlineUsers = new ObservableCollection<UserInfo>();

    private readonly ChatPage _chatPage = new ChatPage();
    private readonly ObservableCollection<ChatVM> _chatVMs = new ObservableCollection<ChatVM>();

    public MainWindowVm()
    {
        SettingsVm.GetInstance().UserChange += (user, serverInfo, added) =>
        {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    if (added)
                    {
                        OnlineUsers.Add(user);
                        _chatVMs.Add(new ChatVM(user, SettingsVm.GetInstance()));
                    }
                    else
                    {
                        for (int i = 0; i < OnlineUsers.Count; i++)
                            if (OnlineUsers[i].Id.Equals(user.Id))
                                OnlineUsers.RemoveAt(i);

                        for (int i = 0; i < _chatVMs.Count; i++)
                            if (_chatVMs[i].User.Id.Equals(user.Id))
                                _chatVMs.RemoveAt(i);
                    }
                        
                })
            );
        };

        SettingsVm.GetInstance().LostConnect += () =>
        {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    OnlineUsers.Clear();
                })
            );
        };

        CurrentPage = new SettingsPage()
        {
            DataContext = SettingsVm.GetInstance()
        };;
    }

    [RelayCommand]
    private void OpenSettings()
    {
        CurrentPage = new SettingsPage()
        {
            DataContext = SettingsVm.GetInstance()
        };
    }
    
    [RelayCommand]
    private void OpenUser(object param)
    {
        UserInfo user = UserInfo.Parse(param.ToString());

        foreach (ChatVM chatVm in _chatVMs)
        {
            if (chatVm.User.Id.Equals(user.Id))
            {
                _chatPage.DataContext = chatVm;
                CurrentPage = _chatPage;
                return;
            }
        }
    }
}