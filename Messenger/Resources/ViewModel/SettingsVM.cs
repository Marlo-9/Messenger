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
using Messenger.Resources.Connection;
using Messenger.Resources.Data;
using Messenger.Resources.View;
using Messenger.Resources.Visual;
using Wpf.Ui;
using Wpf.Ui.Controls;

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
    [ObservableProperty] private bool _isSystemClientStarted = false;
    
    [ObservableProperty] private bool _useIpV6 = false;
    [ObservableProperty] private bool _useCustomServerSettings = false;
    [ObservableProperty] private bool _useAutoFinedServer = false;
    
    [ObservableProperty] private string _textServerStartButton = "Запустить сервер";
    [ObservableProperty] private SymbolIcon _iconServerStartButton = new(SymbolRegular.Play28);
    [ObservableProperty] private ControlAppearance _appearanceServerStartButton = ControlAppearance.Primary;
    
    [ObservableProperty] private string _textClientStartButton = "Запустить клиента";
    [ObservableProperty] private SymbolIcon _iconClientStartButton = new(SymbolRegular.Play28);
    [ObservableProperty] private ControlAppearance _appearanceClientStartButton = ControlAppearance.Secondary;
    
    private ServerObject? _server = null;
    
    public delegate void UserCountChange(UserInfo user, ServerInfo? serverInfo, bool isAdded = true);
    public event UserCountChange UserChange;
    
    public delegate void RemoveConnect();
    public event RemoveConnect LostConnect;

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
        
        Application.Current.MainWindow.Closed += (_, _) =>
        {
            try
            {
                _server?.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        };
    }

    partial void OnUseCustomServerSettingsChanged(bool value)
    {
        if (value)
        {
            IsEnableServerFiled = true;
            IsEnableServerSettings = true;

            IsEnableServerIpFiled = !UseAutoFinedServer;
        }
        else
        {
            IsEnableServerFiled = false;
            IsEnableServerSettings = false;
            IsEnableServerIpFiled = false;
        }
    }

    partial void OnUseAutoFinedServerChanged(bool value)
    {
        IsEnableServerIpFiled = !value;
    }

    partial void OnIsBlockServerSettingsChanged(bool value)
    {
        if (value)
        {
            IsEnableCustomSettings = false;
            IsEnableServerFiled = false;
            IsEnableServerIpFiled = false;
            IsEnableServerSettings = false;
        }
        else
        {
            IsEnableCustomSettings = true;

            IsEnableServerFiled = UseCustomServerSettings;
            IsEnableServerSettings = UseCustomServerSettings;
            IsEnableServerIpFiled = !UseAutoFinedServer && UseCustomServerSettings;
        }
    }

    partial void OnIsServerStartedChanged(bool value)
    {
        if (value)
        {
            IsBlockServerSettings = true;
            TextServerStartButton = "Остановить сервер";
            IconServerStartButton = new SymbolIcon(SymbolRegular.Stop24);
            AppearanceServerStartButton = ControlAppearance.Danger;
        }
        else
        {
            TextServerStartButton = "Запустить сервер";
            IconServerStartButton = new SymbolIcon(SymbolRegular.Play28);
            AppearanceServerStartButton = ControlAppearance.Primary;
            
            LostConnect?.Invoke();
            
            if (!IsSystemClientStarted)
                IsBlockServerSettings = false;
        }
    }

    partial void OnIsSystemClientStartedChanged(bool value)
    {
        IsEnableUserName = !value;
        
        if (value)
        {
            TextClientStartButton = "Остановить клиента";
            IconClientStartButton = new SymbolIcon(SymbolRegular.Stop24);
            AppearanceClientStartButton = ControlAppearance.Danger;
        }
        else
        {
            TextClientStartButton = "Запустить клиента";
            IconClientStartButton = new SymbolIcon(SymbolRegular.Play28);
            AppearanceClientStartButton = ControlAppearance.Secondary;
            
            LostConnect?.Invoke();
        }
        
        if (value) IsBlockServerSettings = true;
    }

    partial void OnUseIpV6Changed(bool value)
    {
        IsEnableAutoFindServer = !value;

        if (value) UseAutoFinedServer = false;
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
        await Writer?.WriteLineAsync(NetworkAssistance.SetMessageType(MessageType.Message, message.ToString()))!;
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
            Console.WriteLine(e.Message);

            try
            {
                _server?.Disconnect();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
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
        if (IsSystemClientStarted)
        {
            await Writer?.WriteLineAsync(NetworkAssistance.SetMessageType(MessageType.CloseConnect))!;
            await Writer?.FlushAsync()!;
            
            Client?.Close();
            
            IsSystemClientStarted = false;
            return;
        }
        
        ServerInfo serverInfo = new ServerInfo();

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
                        Application.Current.MainWindow.Closed += async (_, _) =>
                        {
                            try
                            {
                                await Writer?.WriteLineAsync(NetworkAssistance.SetMessageType(MessageType.CloseConnect))!;
                                await Writer?.FlushAsync()!;
                            
                                Client?.Close();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                            finally
                            {
                                Writer?.Close();
                                Reader?.Close();
                            }
                        };
                    }
                
                    IsSystemClientStarted = true;

                    Reader = new StreamReader(Client.GetStream());
                    Writer = new StreamWriter(Client.GetStream());

                    if (Writer is null || Reader is null) return;

                    await StartSystemClient();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
         
        async Task StartSystemClient()
        {
            await Writer.WriteLineAsync(NetworkAssistance.GetKey());
            await Writer.FlushAsync();
            
            string? answer = await Reader.ReadLineAsync();
            MessageType answerType = NetworkAssistance.GetMessageType(answer);
            
            if (answerType == MessageType.Null)
                throw new Exception("Un correct server answer");
            
            await Writer.WriteLineAsync(NetworkAssistance.SetMessageType(MessageType.SystemClient));
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

                    MessageType messageType = NetworkAssistance.GetMessageType(message);

                    switch (messageType)
                    {
                        case MessageType.NewClient:
                            UserChange?.Invoke(UserInfo.Parse(message), serverInfo);
                            break;
                        case MessageType.RemoveClient:
                            UserChange?.Invoke(UserInfo.Parse(message), null, false);
                            break;
                        case MessageType.SetClientId:
                            User.Id = UserInfo.Parse(message).Id;
                            break;
                        default:
                            Console.WriteLine("Message: " + message);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
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