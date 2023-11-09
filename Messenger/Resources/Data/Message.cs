using System;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Controls;
using TextBlock = Wpf.Ui.Controls.TextBlock;

namespace Messenger.Resources.Data;

public partial class Message : ObservableObject
{
    [ObservableProperty] private string _text;
    [ObservableProperty] private DateTime _sendTime;
    [ObservableProperty] private MessageStatus _sendStatus;
    
    public string Id { get; }
    public static string Title = "Message"; 

    public Message()
    {
        Id = Guid.NewGuid().ToString();
    }

    public Message(string text, DateTime sendTime, MessageStatus status)
    {
        _text = text;
        _sendTime = sendTime;
        _sendStatus = status;
    }
    
    private Message(string id, string text, DateTime sendTime, MessageStatus status)
    {
        Id = id;
        _text = text;
        _sendTime = sendTime;
        _sendStatus = status;
    }

    public static Message Parse(string message)
    {
        string[] values = message.Split(';');

        return new Message(GetValue(values[0]), 
            GetValue(values[1]),
            DateTime.Parse(GetValue(values[2])), 
            Enum.Parse<MessageStatus>(GetValue(values[3])));
    }

    private static string GetValue(string text)
    {
        return text.Split(':')[1];
    }

    public override string ToString()
    {
        return "{" + Title + "}Id:" + Id + ";Text:" + _text + ";SendTime:" + _sendTime + ";SendStatus:" + _sendStatus;
    }
}

public enum MessageStatus
{
    Ship,
    Received,
    Read
}