using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Messenger.Resources.Tools.Connection;

namespace Messenger.Resources.Tools.Data;

[Serializable]
public partial class UserInfo : ObservableObject
{
    [ObservableProperty] private string _id;
    [ObservableProperty] private string _name;
    [ObservableProperty] private UserType _type;
    
    public UserInfo()
    {
    }

    public UserInfo(string name, string id)
    {
        _name = name;
        _id = id;
    }

    public UserInfo(string id, string name, UserType type)
    {
        _id = id;
        _name = name;
        _type = type;
    }

    public override string ToString()
    {
        return "UserName:" + _name + ";UserId:" + _id;
    }

    public string ToLog()
    {
        return "User.Id = " + Id + "\nUser.Name = " + Name;
    }

    public static UserInfo Parse(string? userInfo)
    {
        string[] values = userInfo.Split(';');

        return new UserInfo(GetValue(values[0]), GetValue(values[1]));
    }

    private static string GetValue(string text)
    {
        return text.Split(':')[1];
    }
}

public enum UserType
{
    System,
    Client
}