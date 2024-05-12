using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MessageType
{
    ToServerHandShake = 0,
    ToClientHandShake = 1,
    PingPong = 2,
    Console = 3,
    Position = 4
}

public enum Operations 
{
    Add = 0,
    Substract = 1,
    ShiftRight = 2,
    ShiftLeft = 3
}
public abstract class BaseMessage<T>
{
    public abstract byte[] Serialize();
    public abstract T Deserialize(byte[] message);
    public abstract MessageType GetMessageType();

    public T data;

    public virtual void StarChecksum(List<byte> message)
    {
        uint checkSum = 0;
        uint checkSum2 = 0;

        foreach (byte bytes in message)
        {
            int temp = bytes % 4;

            switch (temp)
            {
                case (int)Operations.Add:

                    checkSum += bytes;
                    checkSum2 += bytes;

                break;

                case (int)Operations.Substract:

                    checkSum -= bytes;
                    checkSum2 -= bytes;

                break;

                case (int)Operations.ShiftRight:

                    checkSum >>= bytes;
                    checkSum2 >>= bytes;

                break;

                case (int)Operations.ShiftLeft:

                    checkSum <<= bytes;
                    checkSum2 <<= bytes;

                break;
            }
        }

        message.AddRange(BitConverter.GetBytes(checkSum));
        message.AddRange(BitConverter.GetBytes(checkSum2));
    }

    public virtual void ReciveChecksum(List<byte> message, out uint sum, out uint sum2) 
    {
        uint checkSum = 0;
        uint checkSum2 = 0;

        foreach (byte bytes in message)
        {
            int temp = bytes % 4;

            switch (temp)
            {
                case (int)Operations.Add:

                    checkSum += bytes;
                    checkSum2 += bytes;

                break;

                case (int)Operations.Substract:

                    checkSum -= bytes;
                    checkSum2 -= bytes;

                break;

                case (int)Operations.ShiftRight:

                    checkSum >>= bytes;
                    checkSum2 >>= bytes;

                break;

                case (int)Operations.ShiftLeft:

                    checkSum <<= bytes;
                    checkSum2 <<= bytes;

                break;
            }
        }

        sum = checkSum - sizeof(uint) * 2; 
        sum2 = checkSum2 - sizeof(uint) * 2; 
    }

    public virtual bool IsChecksumOk(byte[] message) 
    {
        ReciveChecksum(message.ToList<byte>(), out uint sum, out uint sum2);
        
        if (sum == BitConverter.ToUInt32(message, message.Length - sizeof(uint) * 2) &&
            sum2 == BitConverter.ToUInt32(message, message.Length - sizeof(uint)))
        {
            return true;
        }

        else
        {
            return false;
        }
    }
}
public abstract class OrderMessage<T> : BaseMessage<T>
{
    protected static ulong lastSenMsgID = 0;

    protected ulong msjID = 0;

    protected static Dictionary<MessageType, ulong> lastExecutedMsgID = new Dictionary<MessageType, ulong>();
    public abstract MessageType ReadMsgID(byte[] message);
}
public class NetToServerHandShake : OrderMessage<(int, string)>
{
    public override MessageType ReadMsgID(byte[] message)
    {
        MessageType type = (MessageType)BitConverter.ToUInt32(message);

        return type;
    }

    public override (int, string) Deserialize(byte[] message)
    {
        (int, string) outData;

        outData.Item1 = BitConverter.ToInt32(message, 4);

        outData.Item2 = "";
        int messageLenght = BitConverter.ToInt32(message, 8);

        for (int i = 0; i < messageLenght; i++)
        {
            outData.Item2 += (char)message[12 + i];
        }

        return outData;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.ToServerHandShake;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(data.Item1));
        outData.AddRange(BitConverter.GetBytes(data.Item2.Length));

        for (int i = 0; i < data.Item2.Length; i++)
        {
            outData.Add((byte)data.Item2[i]);
        }

        return outData.ToArray();
    }
}
public class NetToClientHandShake : OrderMessage<List<Player>>
{
    public override MessageType ReadMsgID(byte[] message)
    {
        MessageType type = (MessageType)BitConverter.ToUInt32(message);

        return type;
    }

    public override List<Player> Deserialize(byte[] message)
    {
        int currentPosition = 0;
        currentPosition += 4;

        int totalPlayers = BitConverter.ToInt32(message, currentPosition);
        Debug.Log("Players: " + totalPlayers);

        currentPosition += 4;

        List<Player> newPlayerList = new List<Player>();

        for (int i = 0; i < totalPlayers; i++)
        {
            int Id = BitConverter.ToInt32(message, currentPosition);
            currentPosition += 4;

            int clientIdLenght = BitConverter.ToInt32(message, currentPosition);

            string clientId = "";
            currentPosition += 4;

            for (int j = 0; j < clientIdLenght; j++)
            {
                clientId += (char)message[currentPosition];
                currentPosition += 1;
            }

            Debug.Log(clientId + " : " + Id);
            newPlayerList.Add(new Player(Id, clientId));
        }

        return newPlayerList;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.ToClientHandShake;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        outData.AddRange(BitConverter.GetBytes(data.Count));

        for (int i = 0; i < data.Count; i++)
        {
            outData.AddRange(BitConverter.GetBytes(data[i].ID));
            outData.AddRange(BitConverter.GetBytes(data[i].clientId.Length));

            for (int j = 0; j < data[i].clientId.Length; j++)
            {
                outData.Add((byte)data[i].clientId[j]);
            }
        }

        return outData.ToArray();
    }
}
public class NetPingPong : OrderMessage<int>
{
    public override MessageType ReadMsgID(byte[] message)
    {
        MessageType type = (MessageType)BitConverter.ToUInt32(message);

        return type;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.PingPong;
    }


    public override int Deserialize(byte[] message)
    {
        return 0;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        return outData.ToArray();
    }
}
public class NetVector3 : OrderMessage<UnityEngine.Vector3>
{
    public override MessageType ReadMsgID(byte[] message)
    {
        MessageType type = (MessageType)BitConverter.ToUInt32(message);

        return type;
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
        outData.AddRange(BitConverter.GetBytes(lastSenMsgID++));
        outData.AddRange(BitConverter.GetBytes(data.x));
        outData.AddRange(BitConverter.GetBytes(data.y));
        outData.AddRange(BitConverter.GetBytes(data.z));

        return outData.ToArray();
    }
}
public class NetConsole : OrderMessage<(int, string)>
{
    public override MessageType ReadMsgID(byte[] message)
    {
        return (MessageType)BitConverter.ToUInt32(message);
    }

    public override (int, string) Deserialize(byte[] message)
    {
        (int, string) outData;

        int baseByte = 12;

        outData.Item1 = BitConverter.ToInt32(message, 4);

        outData.Item2 = " ";
        int stringLenght = BitConverter.ToInt32(message, 8);

        for (int i = 0; i < stringLenght; i++)
        {
            outData.Item2 += (char)message[baseByte + i];
        }

        return outData;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.Console;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        outData.AddRange(BitConverter.GetBytes(data.Item1));
        outData.AddRange(BitConverter.GetBytes(data.Item2.Length));

        for (int i = 0; i < data.Item2.Length; i++)
        {
            outData.Add((byte)data.Item2[i]);
        }

        return outData.ToArray();
    }
}