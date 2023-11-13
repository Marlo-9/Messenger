using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Messenger.Resources.Tools.Additional;
using Messenger.Resources.Tools.Connection;
using Messenger.Resources.Tools.Data;
using Messenger.Resources.Tools.Enums;
using Messenger.Resources.View;
using Messenger.Resources.Visual;
using Wpf.Ui;
using Wpf.Ui.Controls;
using static Messenger.Resources.Tools.Additional.Logging;

namespace Messenger.Resources.ViewModel;

public partial class SettingsVm : ObservableObject
{
    [ObservableProperty] private UserInfo _user = new UserInfo();
    
    [ObservableProperty] private string _serverIp;
    [ObservableProperty] private string _serverPort;
    
    [ObservableProperty] private bool _isEnableServerFiled = false;
    [ObservableProperty] private bool _isEnableServerIpFiled = false;
    [ObservableProperty] private bool _isEnableServerSettings = false;
    [ObservableProperty] private bool _isEnableCustomSettings = true;
    [ObservableProperty] private bool _isEnableAutoFindServer = true;
    [ObservableProperty] private bool _isEnableUserName = true;
    [ObservableProperty] private bool _isBlockServerSettings = false;
    [ObservableProperty] private bool _isServerStarted= false;
    [ObservableProperty] private bool _isClientStarted = false;
    
    [ObservableProperty] private bool _useIpV6 = false;
    [ObservableProperty] private bool _useCustomServerSettings = false;
    [ObservableProperty] private bool _useAutoFinedServer = false;
    
    [ObservableProperty] private SettingsViewStatus _settingsViewStatus = SettingsViewStatus.EnableAllSettings;
    
    [ObservableProperty] private string _textServerStartButton = "Запустить сервер";
    [ObservableProperty] private SymbolIcon _iconServerStartButton = new(SymbolRegular.Play28);
    [ObservableProperty] private ControlAppearance _appearanceServerStartButton = ControlAppearance.Primary;
    
    [ObservableProperty] private string _textClientStartButton = "Запустить клиента";
    [ObservableProperty] private SymbolIcon _iconClientStartButton = new(SymbolRegular.Play28);
    [ObservableProperty] private ControlAppearance _appearanceClientStartButton = ControlAppearance.Secondary;
    
    private ServerObject? _server = null;
    
    public delegate void AddUserEventArgs(UserInfo user);
    public event AddUserEventArgs AddUser;
    
    public delegate void RemoveUserEventArgs(UserInfo user);
    public event RemoveUserEventArgs RemoveUser;
    
    public delegate void RemoveConnect();
    public event RemoveConnect LostConnect;
    
    public delegate void MessageR(Message message);
    public event MessageR NewMessage;
    
    

    private static SettingsVm? _settingsVm = null;
    private TcpClient? Client = null;
    private StreamReader? Reader;
    private StreamWriter? Writer;

    public static SettingsVm GetInstance()
    {
        return _settingsVm ??= new SettingsVm();
    }
    
    private SettingsVm()
    {
        if (Application.Current.MainWindow == null) return;
        
        Application.Current.MainWindow.Closing += (_, _) =>
        {
            try
            {
                _server?.Disconnect();
            }
            catch (Exception e)
            {
                Logging.GetInstance().Log(e.Message);
            }
        };
    }

    partial void OnSettingsViewStatusChanging(SettingsViewStatus value)
    {
        switch (value)
        {
            case SettingsViewStatus.EnableAllSettings:
                IsEnableUserName = true;
                IsEnableCustomSettings = true;
                IsEnableAutoFindServer = !UseIpV6;
                IsEnableServerIpFiled = UseCustomServerSettings && !UseAutoFinedServer;
                IsEnableServerFiled = UseCustomServerSettings;
                IsEnableServerSettings = UseCustomServerSettings;
                break;
            case SettingsViewStatus.DisableAllSettings:
                IsEnableUserName = false;
                IsEnableCustomSettings = false;
                IsEnableAutoFindServer = false;
                IsEnableServerIpFiled = false;
                IsEnableServerFiled = false;
                IsEnableServerSettings = false;
                break;
            case SettingsViewStatus.ServerStarted:
                IsEnableUserName = true;
                IsEnableCustomSettings = false;
                IsEnableAutoFindServer = false;
                IsEnableServerIpFiled = false;
                IsEnableServerFiled = false;
                IsEnableServerSettings = false;
                break;
            case SettingsViewStatus.ClientStarted:
                IsEnableUserName = false;
                IsEnableCustomSettings = false;
                IsEnableAutoFindServer = false;
                IsEnableServerIpFiled = false;
                IsEnableServerFiled = false;
                IsEnableServerSettings = false;
                break;
            case SettingsViewStatus.AutoFindServerAndCustomSettings:
                IsEnableUserName = true;
                IsEnableCustomSettings = true;
                IsEnableAutoFindServer = true;
                IsEnableServerIpFiled = false;
                IsEnableServerFiled = true;
                IsEnableServerSettings = false;
                break;
            case SettingsViewStatus.AutoFindServer:
                IsEnableUserName = true;
                IsEnableCustomSettings = true;
                IsEnableAutoFindServer = true;
                IsEnableServerIpFiled = false;
                IsEnableServerFiled = false;
                IsEnableServerSettings = false;
                break;
            case SettingsViewStatus.CustomSettings:
                IsEnableUserName = true;
                IsEnableCustomSettings = true;
                IsEnableAutoFindServer = true;
                IsEnableServerIpFiled = true;
                IsEnableServerFiled = true;
                IsEnableServerSettings = true;
                break;
            case SettingsViewStatus.CustomSettingsAndUseIpV6:
                IsEnableUserName = true;
                IsEnableCustomSettings = true;
                IsEnableAutoFindServer = false;
                IsEnableServerIpFiled = true;
                IsEnableServerFiled = true;
                IsEnableServerSettings = true;
                break;
        }
    }

    partial void OnUseAutoFinedServerChanged(bool value)
    {
        if (value)
            SettingsViewStatus = UseCustomServerSettings ? SettingsViewStatus.AutoFindServerAndCustomSettings : SettingsViewStatus.AutoFindServer;
        else
            SettingsViewStatus = UseCustomServerSettings ? SettingsViewStatus.CustomSettings : SettingsViewStatus.EnableAllSettings;
    }

    partial void OnUseCustomServerSettingsChanged(bool value)
    {
        if (value)
        {
            if (UseIpV6)
                SettingsViewStatus = SettingsViewStatus.CustomSettingsAndUseIpV6;
            else
                SettingsViewStatus = UseAutoFinedServer ? SettingsViewStatus.AutoFindServerAndCustomSettings : SettingsViewStatus.CustomSettings;
        }
        else
        {
            SettingsViewStatus = UseAutoFinedServer ? SettingsViewStatus.AutoFindServer : SettingsViewStatus.EnableAllSettings;
        }
    }

    partial void OnUseIpV6Changed(bool value)
    {
        SettingsViewStatus = value ? SettingsViewStatus.CustomSettingsAndUseIpV6 : SettingsViewStatus.CustomSettings;
    }

    partial void OnIsServerStartedChanged(bool value)
    {
        if (value)
        {
            TextServerStartButton = "Остановить сервер";
            IconServerStartButton = new SymbolIcon(SymbolRegular.Stop24);
            AppearanceServerStartButton = ControlAppearance.Danger;

            SettingsViewStatus = SettingsViewStatus.ServerStarted;
        }
        else
        {
            TextServerStartButton = "Запустить сервер";
            IconServerStartButton = new SymbolIcon(SymbolRegular.Play28);
            AppearanceServerStartButton = ControlAppearance.Primary;
            
            LostConnect?.Invoke();
            SettingsViewStatus = SettingsViewStatus.EnableAllSettings;
        }
    }

    partial void OnIsClientStartedChanged(bool value)
    {
        if (value)
        {
            TextClientStartButton = "Остановить клиента";
            IconClientStartButton = new SymbolIcon(SymbolRegular.Stop24);
            AppearanceClientStartButton = ControlAppearance.Danger;
            
            SettingsViewStatus = IsServerStarted ? SettingsViewStatus.DisableAllSettings : SettingsViewStatus.ServerStarted;
        }
        else
        {
            TextClientStartButton = "Запустить клиента";
            IconClientStartButton = new SymbolIcon(SymbolRegular.Play28);
            AppearanceClientStartButton = ControlAppearance.Secondary;
            
            LostConnect?.Invoke();
            SettingsViewStatus = IsServerStarted ? SettingsViewStatus.ServerStarted : SettingsViewStatus.EnableAllSettings;
        }
    }

    private bool CheckValue(out ServerInfo info)
    {
        info = new ServerInfo();

        if (!string.IsNullOrEmpty(ServerIp))
        {
            if (!IPAddress.TryParse(ServerIp, out info.IpAddress))
                return false;
        }
        else
        {
            IPAddress.TryParse(NetworkAssistance.GetIP(), out info.IpAddress);
        }
            
            
        if (!string.IsNullOrEmpty(ServerPort))
        {
            if (!int.TryParse(ServerPort, out info.Port))
                return false;
        }
        else
        {
            info.Port = NetworkAssistance.Port;
        }
        
        return true;
    }

    public async void SendMessage(Message message)
    {
        string text = message.ToJsonString().SetMessageType(NetworkMessageType.Message);
        await Writer?.WriteLineAsync(text)!;
        await Writer?.FlushAsync()!;
    }

    [RelayCommand]
    private async void StartStopServer(object obj)
    {
        try
        {
            if (_server is not null)
            {
                IsServerStarted = false;
                _server.Disconnect();
                _server = null;
                return;
            }

            if (!CheckValue(out ServerInfo serverInfo))
            {
                if (serverInfo.IpAddress is null)
                    await ViewMessage.ShowError((SnackbarPresenter)obj, "Ошибка", "Не корректно введён IP адрес сервера");
                
                if (serverInfo.Port == 0)
                    await ViewMessage.ShowError((SnackbarPresenter)obj, "Ошибка", "Не корректно введён порт сервера");
                
                return;
            }

            if (serverInfo.IpAddress is null)
            {
                await ViewMessage.ShowError((SnackbarPresenter)obj, "Ошибка", "Отсутсвует соединение");
                return;
            }

            _server = new ServerObject(serverInfo.IpAddress, serverInfo.Port, UseIpV6);
            ServerIp = serverInfo.IpAddress.ToString();
            ServerPort = serverInfo.Port.ToString();
            IsServerStarted = true;
            
            await _server.ListenAsync();
        }
        catch (Exception e)
        {
            Logging.GetInstance().Log(e.Message);

            try
            {
                _server?.Disconnect();
            }
            catch (Exception ex)
            {
                Logging.GetInstance().Log(ex.Message);
            }
            finally
            {
                IsServerStarted = false;
                _server = null;
            }
        }
    }
    
    [RelayCommand]
    private async void StartClient(object obj)
    {
        if (IsClientStarted)
        {
            await Writer?.WriteLineAsync("".SetMessageType(NetworkMessageType.CloseConnect))!;
            await Writer?.FlushAsync()!;
            
            Client?.Close();
            
            IsClientStarted = false;
            return;
        }
        
        var serverInfo = new ServerInfo();

        if (string.IsNullOrEmpty(User.Name))
        {
            await ViewMessage.ShowError((SnackbarPresenter)obj, "Ошибка", "Не введено имя пользователя");
            return;
        }

        if (UseAutoFinedServer)
        {
            if (!int.TryParse(ServerPort, out serverInfo.Port))
            {
                await ViewMessage.ShowError((SnackbarPresenter)obj, "Ошибка", "Не корректно введён порт сервера");
                return;
            }
            
            serverInfo.IpAddress = await NetworkAssistance.TryFindServer(serverInfo.Port);
        }
        else
        {
            if (!CheckValue(out serverInfo))
            {
                if (serverInfo.IpAddress is null)
                    await ViewMessage.ShowError((SnackbarPresenter)obj, "Ошибка", "Не корректно введён IP адрес сервера");
                
                if (serverInfo.Port == 0)
                    await ViewMessage.ShowError((SnackbarPresenter)obj, "Ошибка", "Не корректно введён порт сервера");
                
                return;
            }

            if (serverInfo.IpAddress is null)
            {
                await ViewMessage.ShowError((SnackbarPresenter)obj, "Ошибка", "Отсутствует соединение");
                return;
            }
        }

        try
        {
            using (Client = new TcpClient())
            {
                try
                {
                    // Client.Connect(serverInfo.IpAddress, serverInfo.Port);
                    // await NetworkAssistance.RunTask(Client.ConnectAsync(serverInfo.IpAddress, serverInfo.Port), cancellationToken: CancellationToken.None);
                    await NetworkAssistance.ConnectAsync(Client, serverInfo.IpAddress.ToString(), serverInfo.Port,
                        CancellationToken.None);
                    
                    if (Application.Current.MainWindow != null)
                    {
                        Application.Current.MainWindow.Closing += async (_, _) =>
                        {
                            try
                            {
                                await Writer?.WriteLineAsync("".SetMessageType(NetworkMessageType.CloseConnect))!;
                                await Writer?.FlushAsync()!;
                            
                                Client?.Close();
                            }
                            catch (Exception e)
                            {
                                Logging.GetInstance().Log(e.Message);
                            }
                            finally
                            {
                                Writer?.Close();
                                Reader?.Close();
                            }
                        };
                    }
                
                    IsClientStarted = true;

                    Reader = new StreamReader(Client.GetStream());
                    Writer = new StreamWriter(Client.GetStream());

                    if (Writer is null || Reader is null) return;

                    await StartSystemClient();
                }
                catch (Exception ex)
                {
                    Logging.GetInstance().Log(ex.Message);
                }
            }
        }
        catch (Exception e)
        {
            Logging.GetInstance().Log(e.Message);
        }
         
        async Task StartSystemClient()
        {
            await Writer.WriteLineAsync(NetworkAssistance.GetKey());
            await Writer.FlushAsync();
            
            string? answer = await Reader.ReadLineAsync();
            NetworkMessageType answerType = answer.GetMessageType();
            
            if (answerType == NetworkMessageType.Null)
                throw new Exception("Un correct server answer");
            
            await Writer.WriteLineAsync("".SetMessageType(NetworkMessageType.SystemClient));
            await Writer.FlushAsync();
            await Writer.WriteLineAsync(User.Name);
            await Writer.FlushAsync();
            
            await Task.Run(()=>ReceiveMessageAsync(Reader));
        }
        
        async Task ReceiveMessageAsync(StreamReader reader)
        {
            while (true)
            {
                try
                {
                    string? message = await reader.ReadLineAsync();
                    
                    if (string.IsNullOrEmpty(message)) continue;

                    switch (message.GetMessageType())
                    {
                        case NetworkMessageType.NewClient:
                            AddUser?.Invoke(UserInfo.Parse(message));
                            break;
                        case NetworkMessageType.RemoveClient:
                            RemoveUser?.Invoke(UserInfo.Parse(message));
                            break;
                        case NetworkMessageType.SetClientId:
                            User.Id = UserInfo.Parse(message).Id;
                            break;
                        case NetworkMessageType.Message:
                            NewMessage?.Invoke(message.FromJsonString<Message>());
                            break;
                        default:
                            Console.WriteLine("Message: " + message);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logging.GetInstance().Log(e.Message);
                    break;
                }
            }
        }
    }

    [RelayCommand]
    private void CopyServerValue(object obj)
    {
        Clipboard.SetText(((TextBox)obj).Text);
    }

    [RelayCommand]
    private void PasteServerValue(object obj)
    {
        ((TextBox)obj).Text = Clipboard.GetText();
    }
}