using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Messenger.Resources.Tools.Additional;
using Messenger.Resources.Tools.Data;
using Messenger.Resources.Tools.Enums;

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

            await Writer.WriteLineAsync("".SetMessageType(NetworkMessageType.SuccessConnect));
            await Writer.FlushAsync();

            answer = await Reader.ReadLineAsync();

            switch (answer.GetMessageType())
            {
                case NetworkMessageType.CheckClient:
                    server.RemoveConnection(Id);
                    Close();
                    return;
                case NetworkMessageType.SystemClient:
                    User.Type = UserType.System;
                    break;
            }
            
            answer = await Reader.ReadLineAsync();
            User.Name = answer!;

            await server.AddUser(User);
            
            while (client.Connected)
            {
                try
                {
                    answer = await Reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(answer)) continue;
                    
                    switch (answer.GetMessageType())
                    {
                        case NetworkMessageType.CloseConnect:
                            await server.RemoveUser(User);
                            Close();
                            return;
                        case NetworkMessageType.Message:
                            Console.WriteLine(answer.FromJsonString<Message>());
                            await server.BroadcastMessageAsync(User.Id, answer.FromJsonString<Message>());
                            break;
                        default:
                            Console.WriteLine(answer);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logging.GetInstance().Log(e.Message);
                    
                    await server.RemoveUser(User);
                    Close();
                    throw e;
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Logging.GetInstance().Log(e.Message);
            throw e;
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
        
        Logging.GetInstance().Log("User client closed\n" + User.ToLog());
    }
}