using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Messenger.Resources.Connection;

public static class NetworkAssistance
{
    public static int Port { get; } = 5050;

    public static bool CheckConnection()
    {
        return NetworkInterface.GetIsNetworkAvailable();
    }

    public static bool CheckIP(string IPAddress)
    {
        return new Ping().Send(IPAddress).Status == IPStatus.Success;
    }

    public static string? GetIP(bool useIPV6 = false)
    {
        if (!CheckConnection())
            return null;
        
        IPAddress [] addr = Dns.GetHostAddresses(Dns.GetHostName());
        string mask = useIPV6 ? ":" : ".";

        foreach (IPAddress address in addr)
        {
            if (address.ToString().Contains(mask))
                if (CheckIP(address.ToString()))
                    return address.ToString();
        }

        return null;
    }

    public static string GetKey()
    {
        var inputBytes = Encoding.UTF8.GetBytes(DateTime.Now.Date.ToString());
        var inputHash = SHA256.HashData(inputBytes);
        return Convert.ToHexString(inputHash);
    }

    public static bool CheckKey(string key)
    {
        return GetKey().Equals(key);
    }

    public static async Task<IPAddress?> TryFindServer(int port)
    {
        string? currentIp = GetIP();

        if (string.IsNullOrEmpty(currentIp))
            return null;
        
        for (int i = 0; i <= 255; i++)
        {
            using TcpClient client = new TcpClient();
            IPAddress checkIp = IPAddress.Parse(currentIp.Substring(0, currentIp.LastIndexOf('.') + 1) + i);
            StreamReader? reader = null;
            StreamWriter? writer = null;
            
            Console.WriteLine(checkIp.ToString());
         
            try
            {
                await client.ConnectAsync(checkIp, port).WaitAsync(TimeSpan.FromSeconds(2));
            
                reader = new StreamReader(client.GetStream());
                writer = new StreamWriter(client.GetStream());
                writer.AutoFlush = true;

                await writer.WriteLineAsync(GetKey());
            
                string? answer = await reader.ReadLineAsync();
                MessageType answerType = GetMessageType(answer);

                if (answerType == MessageType.Null)
                {
                    writer?.Close();
                    reader?.Close();
                    throw new Exception("Un correct server answer");
                }
                
                if (answerType == MessageType.SuccessConnect)
                {
                    await writer.WriteLineAsync(SetMessageType(MessageType.CheckClient));
                    
                    writer?.Close();
                    reader?.Close();
                    
                    return checkIp;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        
            writer?.Close();
            reader?.Close();
        }

        return null;
    }

    public static string SetMessageType(MessageType type, string message = "")
    {
        return "{" + type + "}" + message;
    }

    public static MessageType GetMessageType(string? message)
    {
        if (!(!string.IsNullOrEmpty(message) &&
              message.Contains('{')&& message.Contains('}') && 
              message.IndexOf('{') < message.IndexOf('}')))
            return MessageType.Null;
        
        return Enum.Parse<MessageType>(message.Substring(1, message.IndexOf('}') - 1));
    }
    
    public static async Task ConnectAsync(this TcpClient tcpClient, string? host, int port, CancellationToken cancellationToken) {
        if (tcpClient == null) {
            throw new ArgumentNullException(nameof(tcpClient));
        }

        cancellationToken.ThrowIfCancellationRequested();

        using (cancellationToken.Register(() => tcpClient.Close())) {
            try {
                cancellationToken.ThrowIfCancellationRequested();

                await tcpClient.ConnectAsync(host, port).ConfigureAwait(false);
            } 
            #if NET5_0_OR_GREATER
            catch (SocketException ex) when (cancellationToken.IsCancellationRequested)
            #elif NETCOREAPP2_0_OR_GREATER
                catch (ObjectDisposedException ex) when (ct.IsCancellationRequested)
            #elif NETFRAMEWORK && NET40_OR_GREATER
                catch (NullReferenceException ex) when (ct.IsCancellationRequested)
            #else
                #error Untested target framework, use with care!
                catch (???) when (ct.IsCancellationRequested)
            #endif
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
    
    /*public static async Task<T> RunTask<T>(Task<T> task, int timeout = 0, CancellationToken cancellationToken = default)
    {
        await RunTask((Task)task, timeout, cancellationToken);
        return await task;
    }
    
    public static async Task RunTask(Task task, int timeout = 0, CancellationToken cancellationToken = default)
    {
        if (timeout == 0) timeout = -1;

        var timeoutTask = Task.Delay(timeout, cancellationToken);
        await Task.WhenAny(task, timeoutTask);

        cancellationToken.ThrowIfCancellationRequested();
        if (timeoutTask.IsCompleted)
            throw new TimeoutException();

        await task;
    }*/
}

public enum MessageType
{
    Null,
    Key,
    SystemClient,
    UserClient,
    CheckClient,
    NewClient,
    RemoveClient,
    SetClientId,
    SuccessConnect,
    CloseConnect,
    Message,
}