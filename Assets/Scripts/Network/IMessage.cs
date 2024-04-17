using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.CompilerServices;
using UnityEngine.UI;

public enum MessageType
{
    HandShake = -1,
    Console = 0,
    Position = 1,
    String = 2
}

public abstract class BaseMessage<T>
{
    public abstract MessageType GetMessageType();
    public abstract byte[] Serialize();
    public abstract T Deserialize(byte[] message);

    public T data;
}

public abstract class OrderMessage<T> : BaseMessage<T>
{
    protected static ulong lastSenMsgID = 0;

    protected static ulong msjID = 0;

    protected static Dictionary<MessageType, ulong> lastExecutedMsgID = new Dictionary<MessageType, ulong>();
    public abstract void ReadMsgID(byte[] message);
}

public abstract class NetHandShake : OrderMessage<(long, int)>
{
    public override void ReadMsgID(byte[] message) 
    {

    }

    public override (long, int) Deserialize(byte[] message)
    {
        (long, int) outData;

        outData.Item1 = BitConverter.ToInt64(message, 4);
        outData.Item2 = BitConverter.ToInt32(message, 12);

        return outData;
    }

    public override MessageType GetMessageType()
    {
       return MessageType.HandShake;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(data.Item1));
        outData.AddRange(BitConverter.GetBytes(data.Item2));

        return outData.ToArray();
    }
}

public abstract class NetVector3 : OrderMessage<UnityEngine.Vector3>
{
    private static ulong lastMsgID = 0;

    public NetVector3(Vector3 data)
    {
        this.data = data;
    }

    public override void ReadMsgID(byte[] message)
    {

    }

    public override Vector3 Deserialize(byte[] message)
    {
        Vector3 outData;

        outData.x = BitConverter.ToSingle(message, 8);
        outData.y = BitConverter.ToSingle(message, 12);
        outData.z = BitConverter.ToSingle(message, 16);

        return outData;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.Position;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(lastMsgID++));
        outData.AddRange(BitConverter.GetBytes(data.x));
        outData.AddRange(BitConverter.GetBytes(data.y));
        outData.AddRange(BitConverter.GetBytes(data.z));

        return outData.ToArray();
    }

    //Dictionary<Client,Dictionary<msgType,int>>
}

public class NetString : OrderMessage<string>
{
    public NetString(string data)
    {
        this.data = data;
    }

    public override void ReadMsgID(byte[] message)
    {

    }

    public override string Deserialize(byte[] message)
    {
        string outData;
        
        int stringLength = BitConverter.ToInt32(message, 4);

        outData = BitConverter.ToString(message, 4);

        for (int i = 0; i < stringLength; i++)
        {
            outData += (char)message[8 + i]; 
        }

        Debug.Log(outData);

        return outData;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.String;
    }

    public override byte[] Serialize()  
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(data.Length));

        for (int i = 0; i < data.Length; i++)
        {
            outData.Add((byte)data[i]);
        }

        Debug.Log(outData);

        return outData.ToArray();
    }
}