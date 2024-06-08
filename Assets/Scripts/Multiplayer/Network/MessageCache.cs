using System;
using System.Collections.Generic;

[Serializable]
public class MessageCache
{
    public MessageType type;
    public List<byte> data;
    public ulong messageId;

    public float timerForDelete = 0.0f;
    public float timerForResend = 0.0f;
    public bool startTimer = false;
    public bool canBeResended = false;

    public MessageCache(MessageType newtype, List<byte> newdata, ulong newmessageId)
    {
        type = newtype;
        data = newdata;
        messageId = newmessageId;
        timerForDelete = 0.0f;
        timerForResend = 0.0f;
        startTimer = false;
    }

    public MessageCache(MessageType newtype, ulong newmessageId)
    {
        type = newtype;
        messageId = newmessageId;
        timerForDelete = 0.0f;
        timerForResend = 0.0f;
        startTimer = false;
    }
}