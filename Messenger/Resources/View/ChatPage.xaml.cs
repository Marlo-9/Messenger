using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Messenger.Resources.Connection;
using Messenger.Resources.Data;

namespace Messenger.Resources.View;

public partial class ChatPage : Page
{
    public ChatPage()
    {
        InitializeComponent();
    }
}