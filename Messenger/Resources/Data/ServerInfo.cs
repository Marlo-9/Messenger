using System.Net;

namespace Messenger.Resources.Data;

public class ServerInfo
{
    public IPAddress? IpAddress;
    public int Port = 0;

    public ServerInfo()
    {
    }

    public ServerInfo(IPAddress ipAddress, int port)
    {
        IpAddress = ipAddress;
        Port = port;
    }
    
    public ServerInfo(string ipAddress, string port)
    {
        IpAddress = IPAddress.Parse(ipAddress);
        Port = int.Parse(port);
    }
}