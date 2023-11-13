using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Controls;

namespace Messenger.Resources.Tools.Data;

public partial class MessageInfo : ObservableObject
{
    public string Id { get; } = Guid.NewGuid().ToString();
    
    [ObservableProperty] private string _recipientId;
    [ObservableProperty] private DateTime _sendTime;
    [ObservableProperty] private MessageStatus _sendStatus;

    public MessageInfo()
    {
        _sendTime = DateTime.Now;
    }

    public MessageInfo(string recipientId)
    {
        _recipientId = recipientId;
        _sendTime = DateTime.Now;
        _sendStatus = MessageStatus.Ship;
    }

    public MessageInfo(string recipientId, DateTime sendTime)
    {
        _recipientId = recipientId;
        _sendTime = sendTime;
        _sendStatus = MessageStatus.Ship;
    }

    public MessageInfo(string recipientId, DateTime sendTime, MessageStatus sendStatus)
    {
        _recipientId = recipientId;
        _sendTime = sendTime;
        _sendStatus = sendStatus;
    }
}