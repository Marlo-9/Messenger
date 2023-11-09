using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Messenger.Resources.Data;

namespace Messenger.Resources.Connection;

class ClientObject
{
    protected internal string Id { get; } = Guid.NewGuid().ToString();
    protected internal StreamWriter Writer { get;}
    protected internal StreamReader Reader { get;}
 
    TcpClient client;
    ServerObject server;
    public UserInfo User { get; private set; } = new UserInfo();
    public bool IsSystem { get; private set; }
 
    public ClientObject(TcpClient tcpClient, ServerObject serverObject)
    {
        client = tcpClient;
        server = serverObject;
        
        var stream = client.GetStream();
        User.Id = Id;
        
        Reader = new StreamReader(stream);
        Writer = new StreamWriter(stream);
    }
 
    public async Task ProcessAsync()
    {
        try
        {
            string? answer = await Reader.ReadLineAsync();
            
            if (answer is null || !NetworkAssistance.CheckKey(answer))
                throw new Exception("Uncorrect Key");

            await Writer.WriteLineAsync(NetworkAssistance.SetMessageType(MessageType.SuccessConnect));
            await Writer.FlushAsync();

            answer = await Reader.ReadLineAsync();

            switch (NetworkAssistance.GetMessageType(answer))
            {
                case MessageType.CheckClient:
                    server.RemoveConnection(Id);
                    Close();
                    return;
                case MessageType.SystemClient:
                    User.Type = UserType.System;
                    break;
                case MessageType.UserClient:
                    User.Type = UserType.Client;
                    break;
            }
            
            answer = await Reader.ReadLineAsync();
            User.Name = answer!;
            
            string? message = "";
            await server.ChangeUsers(User);
            
            while (true)
            {
                try
                {
                    message = await Reader.ReadLineAsync();
                    if (message == null) continue;
                    if (NetworkAssistance.GetMessageType(message) == MessageType.CloseConnect)
                    {
                        await server.ChangeUsers(User, false);
                        
                        server.RemoveConnection(User.Id);
                        
                        Close();
                        
                        return;
                    }
                    
                    message = $"{User.Name}: {message}";
                    Console.WriteLine(message);
                }
                catch
                {
                    await server.ChangeUsers(User, false);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            server.RemoveConnection(Id);
        }
    }
    protected internal void Close()
    {
        Writer.Close();
        Reader.Close();
        client.Close();
    }
}

/*class ClientObject
{
    protected internal string Id { get; } = Guid.NewGuid().ToString();
    protected internal string Name { get; }
    protected internal StreamWriter Writer { get;}
    protected internal StreamReader Reader { get;}
 
    TcpClient client;
    ServerObject server; // объект сервера
    UserInfo user;
    
    public ClientObject(TcpClient tcpClient, ServerObject serverObject)
    {
        client = tcpClient;
        server = serverObject;
        
        var stream = client.GetStream();
        
        Reader = new StreamReader(stream);
        Writer = new StreamWriter(stream);
    }
 
    public async Task ProcessAsync()
    {
        try
        {
            string? key = await Reader.ReadLineAsync();
            
            if (key is null || !NetworkAssistance.CheckKey(key))
                throw new Exception("Uncorrect Key");
            
            string? userName = await Reader.ReadLineAsync();

            user = new UserInfo(userName, Id);
            
            //await server.NewUserAdd(user);
            
            // в бесконечном цикле получаем сообщения от клиента
            string message;
            while (true)
            {
                try
                {
                    message = await Reader.ReadLineAsync();
                    if (message == null) continue;
                    Console.WriteLine(message);
                    await server.BroadcastMessageAsync(message, Id);
                }
                catch (Exception e)
                {
                    message = $"{userName} покинул чат";
                    Console.WriteLine(e.Message);
                    Console.WriteLine(message);
                    await server.BroadcastMessageAsync(message, Id);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            server.RemoveConnection(Id);            // в случае выхода из цикла закрываем ресурсы
        }
    }
    // закрытие подключения
    protected internal void Close()
    {
        Writer.Close();
        Reader.Close();
        client.Close();
    }
}*/