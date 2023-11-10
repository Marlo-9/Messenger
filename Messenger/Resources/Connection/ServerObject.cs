using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Messenger.Resources.Data;

namespace Messenger.Resources.Connection;

class ServerObject
{
    TcpListener tcpListener;
    ObservableCollection<ClientObject> clients = new ObservableCollection<ClientObject>();

    public ServerObject(string ip, int port, bool isUseIpv6 = false)
    {
        tcpListener = new TcpListener(IPAddress.Parse(ip), port);
    }
    
    public ServerObject(IPAddress ip, int port, bool isUseIpv6 = false)
    {
        tcpListener = new TcpListener(ip, port);
    }

    public ServerObject(IPAddress ipAddress)
    {
        tcpListener = new TcpListener(ipAddress, NetworkAssistance.Port);
    }

    protected internal void RemoveConnection(string id)
    {
        ClientObject? client = clients.FirstOrDefault(c => c.Id == id);
        if (client != null) clients.Remove(client);
        client?.Close();
    }
    protected internal async Task ListenAsync()
    {
        try
        {
            tcpListener.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений...");
 
            while (true)
            {
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
 
                ClientObject clientObject = new ClientObject(tcpClient, this);
                clients.Add(clientObject);
                Task.Run(clientObject.ProcessAsync);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Disconnect();
        }
    }
    protected internal async Task BroadcastMessageAsync(string senderId, Message message)
    {
        foreach (var client in clients)
        {
            if (client.Id == message.RecipientId)
            {
                await client.Writer.WriteLineAsync(message.ToString());
                await client.Writer.FlushAsync();
                
                return;
            }
        }
    }
    
    protected internal async Task ChangeUsers(UserInfo info, bool isNewUser = true)
    {
        if (isNewUser)
        {
            Console.WriteLine("New client: " + info.Name);

            ClientObject newClient = null;

            foreach (var client in clients)
                if (client.User.Id == info.Id)
                    newClient = client;

            foreach (var client in clients)
            {
                if (client.User.Id != info.Id)
                {
                    await client.Writer.WriteLineAsync(NetworkAssistance.SetMessageType(MessageType.NewClient, info.ToString()));
                    await newClient?.Writer.WriteLineAsync(NetworkAssistance.SetMessageType(MessageType.NewClient, client.User.ToString()))!;
                }
                else
                    await client.Writer.WriteLineAsync(NetworkAssistance.SetMessageType(MessageType.SetClientId, info.ToString()));
            
                await client.Writer.FlushAsync();
            }
        }
        else
        {
            Console.WriteLine("Remove client: " + info.Name);

            foreach (var client in clients)
            {
                if (client.User.Id != info.Id)
                    await client.Writer.WriteLineAsync(NetworkAssistance.SetMessageType(MessageType.RemoveClient, info.ToString()));
            
                await client.Writer.FlushAsync();
            }
        }
        
    }
    protected internal void Disconnect()
    {
        foreach (var client in clients)
            client.Close();
        
        tcpListener.Stop();
    }
}

/*class ServerObject
{
    
    TcpListener tcpListener = new TcpListener(IPAddress.Parse(NetworkAssistance.IPAddresses[0]), NetworkAssistance.Port);
    List<ClientObject> clients = new List<ClientObject>();
    List<UserInfo> Users = new List<UserInfo>();
    
    /// <summary>
    /// Отключение клиента по Id
    /// </summary>
    /// <param name="id">Id клиента</param>
    protected internal void RemoveConnection(string id)
    {
        ClientObject? client = clients.FirstOrDefault(c => c.Id == id);
        
        if (client != null) clients.Remove(client);
        
        client?.Close();
    }
    
    /// <summary>
    /// Прослушивание входящих подключений
    /// </summary>
    protected internal async Task ListenAsync()
    {
        try
        {
            tcpListener.Start();
            Console.WriteLine($"Сервер запущен {tcpListener.Server.LocalEndPoint}");
 
            while (true)
            {
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
 
                ClientObject clientObject = new ClientObject(tcpClient, this);
                
                clients.Add(clientObject);
                
                Task.Run(clientObject.ProcessAsync);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Disconnect();
        }
    }
 
    /// <summary>
    /// Трансляция сообщения подключенным клиентам
    /// </summary>
    /// <param name="message"></param>
    /// <param name="id"></param>
    protected internal async Task BroadcastMessageAsync(string message, string id)
    {
        foreach (var client in clients)
        {
            if (client.Id != id) // если id клиента не равно id отправителя
            {
                await client.Writer.WriteLineAsync(message); //передача данных
                await client.Writer.FlushAsync();
            }
        }
    }

    protected internal async Task NewUserAdd(UserInfo info)
    {
        Console.WriteLine("New user: " + info.UserName);
        foreach (var client in clients)
        {
            //if (info.UserId != client.Id)
            //{
                await client.Writer.WriteLineAsync(info.ToString()); //передача данных
                await client.Writer.FlushAsync();
            //}
        }
    }
    
    /// <summary>
    /// Отключение всех пользоватлей
    /// </summary>
    protected internal void Disconnect()
    {
        foreach (var client in clients)
            client.Close(); 
        
        tcpListener.Stop();
    }
}*/