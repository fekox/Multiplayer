using System.Net;
using UnityEngine.Events;

public interface IMessageChecker
{
    public UnityEvent<byte[], IPEndPoint> OnPreviousData { get; protected set; }
    void CheckImportantMessageConfirmation((MessageType, ulong) data);
    bool IsTheNextMessage(MessageType messageType, MessageCache value);
    void CheckPendingMessages(MessageType messageType, ulong value);
}

