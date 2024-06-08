using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public static class Utilities
{
    public static int Sorter(MessageCache cache1, MessageCache cache2)
    {
        return cache1.messageId > cache2.messageId ? (int)cache1.messageId : (int)cache2.messageId;
    }
}

public class Client
{
    public string clientName;
    public float timeStamp;
    public int id;

    public DateTime timer = DateTime.UtcNow;
    public bool connected;
    public IPEndPoint ipEndPoint;

    public Dictionary<MessageType, MessageCache> lastReceiveMessage = new Dictionary<MessageType, MessageCache>();
    public Dictionary<MessageType, List<MessageCache>> pendingMessages = new Dictionary<MessageType, List<MessageCache>>();
    public List<MessageCache> lastImportantMessages = new List<MessageCache>();

    public Client(IPEndPoint ipEndPoint, int id, float timeStamp)
    {
        clientName = "";
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;

        this.connected = true;
    }

    public int GetClientID()
    {
        return id;
    }

    public void SetClientID(int newID)
    {
        this.id = newID;
    }

    public void PassClientData(Client newClient)
    {
        newClient.id = id;
        newClient.timeStamp = timeStamp;
        newClient.ipEndPoint = ipEndPoint;
    }

    public IPEndPoint GetIp()
    {
        return ipEndPoint;
    }

    public string GetClientName()
    {
        return clientName;
    }

    public void ResetTimer() 
    {
        this.timer = DateTime.UtcNow;
    }

    public MessageCache GetLastMessage(MessageType msg)
    {
        return lastReceiveMessage[msg];
    }

    public bool IsTheLastMesagge(MessageType messageType, MessageCache msgToCache)
    {
        if (lastReceiveMessage.TryAdd(messageType, msgToCache))
        {
            return true;
        }

        if (lastReceiveMessage[messageType].messageId < msgToCache.messageId)
        {
            lastReceiveMessage[messageType] = msgToCache;
            return true;
        }

        return true;
    }

    public bool IsTheNextMessage(MessageType messageType, MessageCache value)
    {
        if (lastReceiveMessage.TryAdd(messageType, value))
        {
            return true;
        }

        if (lastReceiveMessage[messageType].messageId + 1 == value.messageId)
        {
            lastReceiveMessage[messageType] = value;
            CheckPendingMessages(messageType, value.messageId);

            return true;
        }
        else
        {
            pendingMessages.TryAdd(messageType, new List<MessageCache>());
            pendingMessages[messageType].Add(new MessageCache(messageType, value.messageId));
            pendingMessages[messageType].Sort(Utilities.Sorter);
            return false;
        }
    }

    public void CheckPendingMessages(MessageType messageType, ulong value)
    {
        if (pendingMessages.ContainsKey(messageType) && pendingMessages[messageType].Count > 0)
        {
            pendingMessages[messageType].Sort(Utilities.Sorter);
            if (value - pendingMessages[messageType][0].messageId + 1 == 0)
            {
                pendingMessages[messageType][0].data.ToArray();
                pendingMessages[messageType].RemoveAt(0);
            }
        }
    }

    public void CheckImportantMessageConfirmation((MessageType, ulong) data)
    {
        foreach (var cached in lastImportantMessages)
        {
            //  Debug.Log($"Id Comparison {cached.messageId} & {data.Item2}");
            if (cached.messageId == data.Item2 && cached.type == data.Item1)
            {
                cached.startTimer = true;
                cached.canBeResended = false;
                Debug.Log($"Confirmation from client {id} of {cached.type} with id {cached.messageId} was received.");
                lastImportantMessages?.Remove(cached);
                break;
            }
        }
    }
    public void OnDestroy()
    {
        lastImportantMessages.Clear();
        pendingMessages.Clear();
        lastReceiveMessage.Clear();
    }
}
