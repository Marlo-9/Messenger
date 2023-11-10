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
    [ObservableProperty] private string _recipientId;
    [ObservableProperty] private DateTime _sendTime;
    [ObservableProperty] private MessageStatus _sendStatus;

    public string Id { get; } = Guid.NewGuid().ToString();
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

    public Message(string text)
    {
        _text = text;
        _sendTime = DateTime.Now;
        _sendStatus = MessageStatus.Ship;
    }

    public Message(string recipientId, string text)
    {
        _recipientId = recipientId;
        _text = text;
        _sendTime = DateTime.Now;
        _sendStatus = MessageStatus.Ship;
    }
    
    private Message(string id, string recipientId, string text, DateTime sendTime, MessageStatus status)
    {
        Id = id;
        _recipientId = recipientId;
        _text = text;
        _sendTime = sendTime;
        _sendStatus = status;
    }

    public static Message Parse(string message)
    {
        string[] values = message.Split(';');

        return new Message(GetValue(values[0]), 
            GetValue(values[1]),
            GetValue(values[2]),
            DateTime.Parse(GetValue(values[3]).Replace('/', ':')), 
            Enum.Parse<MessageStatus>(GetValue(values[4])));
    }

    private static string GetValue(string text)
    {
        return text.Split(':')[1];
    }

    public override string ToString()
    {
        return "Id:" + Id + ";RecipientId:" + _recipientId + ";Text:" + _text + ";SendTime:" + _sendTime.ToString().Replace(':', '/') + ";SendStatus:" + _sendStatus;
    }
}

public enum MessageStatus
{
    Ship,
    Received,
    Read
}