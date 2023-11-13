using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Messenger.Resources.Tools.Additional;
using Messenger.Resources.Tools.Data;
using Messenger.Resources.Tools.Enums;
using Messenger.Resources.View;

namespace Messenger.Resources.ViewModel;

public partial class MainWindowVm : ObservableObject
{
    [ObservableProperty] private string _title = "Мессенджер";
    [ObservableProperty] private Page _currentPage;
    [ObservableProperty] private ObservableCollection<UserInfo> _onlineUsers = new ObservableCollection<UserInfo>();

    private readonly ChatPage _chatPage = new ChatPage();
    private readonly ObservableCollection<ChatVm> _chatVMs = new ObservableCollection<ChatVm>();

    public MainWindowVm()
    {
        Logging.GetInstance().StartSession();
        
        SettingsVm.GetInstance().AddUser += (user) =>
        {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    OnlineUsers.Add(user);
                    _chatVMs.Add(new ChatVm(user, SettingsVm.GetInstance()));
                })
            );
        };
        
        SettingsVm.GetInstance().RemoveUser += (user) =>
        {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    if (CurrentPage.DataContext is ChatVm dataContext)
                        if (dataContext.User.Id.Equals(user.Id))
                            CurrentPage = new SettingsPage() { DataContext = SettingsVm.GetInstance() };
                    
                    for (var i = 0; i < OnlineUsers.Count; i++)
                        if (OnlineUsers[i].Id.Equals(user.Id))
                            OnlineUsers.RemoveAt(i);

                    for (var i = 0; i < _chatVMs.Count; i++)
                        if (_chatVMs[i].User.Id.Equals(user.Id))
                            _chatVMs.RemoveAt(i);
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
        };
    }

    [RelayCommand]
    private void OpenSettings()
    {
        Logging.GetInstance().Log("Open settings page");
        
        CurrentPage = new SettingsPage()
        {
            DataContext = SettingsVm.GetInstance()
        };
    }
    
    [RelayCommand]
    private void OpenUser(object param)
    {
        var user = UserInfo.Parse(param.ToString());

        foreach (var chatVm in _chatVMs)
        {
            if (!chatVm.User.Id.Equals(user.Id)) continue;
            
            _chatPage.DataContext = chatVm;
            CurrentPage = _chatPage;
            Logging.GetInstance().Log("Open user page" + user.ToLog());
            
            return;
        }
    }
}