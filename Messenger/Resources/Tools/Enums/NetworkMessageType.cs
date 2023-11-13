namespace Messenger.Resources.Tools.Enums;

public enum NetworkMessageType
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
    Message
}