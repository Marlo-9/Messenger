using System;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Controls;
using TextBlock = Wpf.Ui.Controls.TextBlock;

namespace Messenger.Resources.Tools.Data;

public partial class Message : ObservableObject
{
    [JsonIgnore]
    [ObservableProperty] private object _content;
    [ObservableProperty] private string _text;
    [ObservableProperty] private MessageInfo _info;
    [ObservableProperty] private ControlAppearance _controlAppearance;

    public Message()
    {
        _info = new MessageInfo();
    }

    public Message(string text, MessageInfo info)
    {
        _text = text;
        _info = info;
    }

    public Message(object content, MessageInfo info)
    {
        _content = content;
        _info = info;
    }

    public Message(string text, string recipientId)
    {
        _text = text;

        _info = new MessageInfo(recipientId);
    }

    public Message(string text, string recipientId, DateTime sendTime)
    {
        _text = text;

        _info = new MessageInfo(recipientId, sendTime);
    }

    public Message(string text, string recipientId, DateTime sendTime, MessageStatus sendStatus)
    {
        _text = text;

        _info = new MessageInfo(recipientId, sendTime, sendStatus);
    }
}

public enum MessageStatus
{
    Ship,
    Received,
    Read
}