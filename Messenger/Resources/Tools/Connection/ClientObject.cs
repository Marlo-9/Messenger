using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Messenger.Resources.Tools.Data;

namespace Messenger.Resources.Tools.Connection;

class ClientObject
{
    protected internal string Id { get; } = Guid.NewGuid().ToString();
    protected internal StreamWriter Writer { get;}
    protected internal StreamReader Reader { get;}

    private string? _chatId;
 
    TcpClient client;
    ServerObject server;
    public UserInfo User { get; private set; } = new UserInfo();
 
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
            }
            
            answer = await Reader.ReadLineAsync();
            User.Name = answer!;

            Task.Delay(100);            
            await server.ChangeUsers(User);
            
            while (client.Connected)
            {
                try
                {
                    answer = await Reader.ReadLineAsync();
                    if (answer == null) continue;
                    
                    switch (NetworkAssistance.GetMessageType(answer))
                    {
                        case MessageType.CloseConnect:
                            await server.ChangeUsers(User, false);
                            server.RemoveConnection(User.Id);
                            Close();
                            return;
                        case MessageType.Message:
                            await server.BroadcastMessageAsync(User.Id, Message.Parse(answer));
                            break;
                        default:
                            Console.WriteLine(answer);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
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