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
    Position = 4,
    SameName = 5,
    MaxPlayers = 6,
    Timer = 7
}

public enum Operations
{
    Add = 0,
    Substract = 1,
    ShiftRight = 2,
    ShiftLeft = 3
}

public enum MessageFlags 
{
    Ordenable = 0,
    Important = 1,
    Resend = 2
}
public abstract class BaseMessage<T>
{
    public T data;
    public static int playerID;
    public abstract byte[] Serialize(int newPlayer);
    public abstract T Deserialize(byte[] message);
    public abstract MessageType GetMessageType();
    public virtual void StartChecksum(List<byte> message)
    {
        uint checkSum = 0;
        uint checkSum2 = 0;

        int messageLenght = message.Count;

        for (int i = 0; i < messageLenght; i++)
        {
            int temp = message[i] % 4;

            switch (temp)
            {
                case (int)Operations.Add:

                    checkSum += message[i];
                    checkSum2 += message[i];

                break;

                case (int)Operations.Substract:

                    checkSum -= message[i];
                    checkSum2 -= message[i];

                break;

                case (int)Operations.ShiftRight:

                    checkSum >>= message[i];
                    checkSum2 >>= message[i];

                break;

                case (int)Operations.ShiftLeft:

                    checkSum <<= message[i];
                    checkSum2 <<= message[i];

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

        int messageLenght = message.Count - sizeof(uint) * 2;

        for (int i = 0; i < messageLenght; i++)
        {
            int temp = message[i] % 4;

            switch (temp)
            {
                case (int)Operations.Add:

                    checkSum += message[i];
                    checkSum2 += message[i];

                    break;

                case (int)Operations.Substract:

                    checkSum -= message[i];
                    checkSum2 -= message[i];

                    break;

                case (int)Operations.ShiftRight:

                    checkSum >>= message[i];
                    checkSum2 >>= message[i];

                    break;

                case (int)Operations.ShiftLeft:

                    checkSum <<= message[i];
                    checkSum2 <<= message[i];

                    break;
            }
        }

        sum = checkSum;
        sum2 = checkSum2;
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

    public virtual byte[] Serialize()
    {
        return Serialize(playerID);
    }
}
public abstract class OrderMessage<T> : BaseMessage<T>
{
    public int messageID = 0;
    public int offsetSize = 0;

    protected virtual void BasicSerialize(List<byte> outData, MessageType messageType, int newPlayerID, MessageFlags messageFlags)
    {
        outData.AddRange(BitConverter.GetBytes((int)messageType));
        outData.AddRange(BitConverter.GetBytes(newPlayerID));
        outData.AddRange(BitConverter.GetBytes((int)messageFlags));
        outData.AddRange(BitConverter.GetBytes(messageID++));
    }

    public abstract MessageType ReadMsgID(byte[] message);

    public abstract MessageFlags GetMessageFlag();
}
public class NetToServerHandShake : OrderMessage<(int, string)>
{
    public override MessageType ReadMsgID(byte[] message)
    {
        MessageType type = (MessageType)BitConverter.ToUInt32(message);

        return type;
    }

    public override MessageFlags GetMessageFlag() 
    {
        return MessageFlags.Important;
    }

    public override (int, string) Deserialize(byte[] message)
    {
        (int, string) outData;

        offsetSize = 16;

        outData.Item1 = BitConverter.ToInt32(message, offsetSize);
        
        offsetSize += 4;
        outData.Item2 = " ";
        int messageLenght = BitConverter.ToInt32(message, offsetSize);

        offsetSize += 4;

        for (int i = 0; i < messageLenght; i++)
        {
            outData.Item2 += (char)message[offsetSize + i];
        }

        return outData;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.ToServerHandShake;
    }

    public override byte[] Serialize(int newPlayer)
    {
        List<byte> outData = new List<byte>();

        BasicSerialize(outData, GetMessageType(), data.Item1, GetMessageFlag());

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

    public override MessageFlags GetMessageFlag()
    {
        return MessageFlags.Important;
    }

    public override List<Player> Deserialize(byte[] message)
    {
        offsetSize = 16;

        int totalPlayers = BitConverter.ToInt32(message, offsetSize);
        Debug.Log("Players: " + totalPlayers);

        List<Player> newPlayerList = new List<Player>();

        for (int i = 0; i < totalPlayers; i++)
        {
            int Id = BitConverter.ToInt32(message, 4 + offsetSize);

            int clientIdLenght = BitConverter.ToInt32(message, 8 + offsetSize);

            Debug.Log("Name Lenght:" + clientIdLenght);

            string clientId = "";

            for (int j = 1; j < clientIdLenght; j++)
            {
                clientId += (char)message[12 + offsetSize + j];
            }

            offsetSize += clientIdLenght + 8;

            Debug.Log(clientId + " || ID:" + Id);
            newPlayerList.Add(new Player(Id, clientId));
        }

        return newPlayerList;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.ToClientHandShake;
    }

    public override byte[] Serialize(int newPlayer)
    {
        List<byte> outData = new List<byte>();

        BasicSerialize(outData, GetMessageType(), newPlayer, GetMessageFlag());

        outData.AddRange(BitConverter.GetBytes(data.Count));

        for (int i = 0; i < data.Count; i++)
        {
            outData.AddRange(BitConverter.GetBytes(data[i].ID));
            outData.AddRange(BitConverter.GetBytes(data[i].tagName.Length));

            for (int j = 0; j < data[i].tagName.Length; j++)
            {
                outData.Add((byte)data[i].tagName[j]);
            }
        }

        return outData.ToArray();
    }
}
public class NetPingPong : OrderMessage<int>
{
    public override MessageType ReadMsgID(byte[] message)
    {
        return (MessageType)BitConverter.ToUInt32(message);
    }

    public override MessageType GetMessageType()
    {
        return MessageType.PingPong;
    }

    public override MessageFlags GetMessageFlag()
    {
        return MessageFlags.Important;
    }


    public override int Deserialize(byte[] message)
    {
        int outData;

        offsetSize = 16;

        outData = BitConverter.ToInt32(message, offsetSize);

        return outData;
    }

    public override byte[] Serialize(int newPlayer)
    {
        List<byte> outData = new List<byte>();

        BasicSerialize(outData, GetMessageType(), newPlayer, GetMessageFlag());

        outData.AddRange(BitConverter.GetBytes(data));

        StartChecksum(outData);

        return outData.ToArray();
    }
}
public class NetVector3 : OrderMessage<(int id, UnityEngine.Vector3)>
{
    public override MessageType ReadMsgID(byte[] message)
    {
        return (MessageType)BitConverter.ToUInt32(message);
    }

    public override MessageFlags GetMessageFlag()
    {
        return MessageFlags.Ordenable;
    }

    public override (int id, UnityEngine.Vector3) Deserialize(byte[] message)
    {
        (int id, UnityEngine.Vector3) outData;

        offsetSize = 16;

        outData.Item1 = BitConverter.ToInt32(message, offsetSize);

        offsetSize += 4;
        outData.Item2.x = BitConverter.ToSingle(message, offsetSize);

        offsetSize += 4;
        outData.Item2.y = BitConverter.ToSingle(message, offsetSize);

        offsetSize += 4;
        outData.Item2.z = BitConverter.ToSingle(message, offsetSize);

        return outData;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.Position;
    }

    public override byte[] Serialize(int newPlayer)
    {
        List<byte> outData = new List<byte>();
        
        BasicSerialize(outData, GetMessageType(), newPlayer, GetMessageFlag());

        outData.AddRange(BitConverter.GetBytes(data.id));
        outData.AddRange(BitConverter.GetBytes(data.Item2.x));
        outData.AddRange(BitConverter.GetBytes(data.Item2.y));
        outData.AddRange(BitConverter.GetBytes(data.Item2.z));

        StartChecksum(outData);

        return outData.ToArray();
    }
}
public class NetConsole : OrderMessage<(int, string)>
{
    public override MessageType ReadMsgID(byte[] message)
    {
        return (MessageType)BitConverter.ToUInt32(message);
    }

    public override MessageFlags GetMessageFlag()
    {
        return MessageFlags.Ordenable;
    }

    public override (int, string) Deserialize(byte[] message)
    {
        (int, string) outData;

        offsetSize = 16; 

        outData.Item1 = BitConverter.ToInt32(message, offsetSize);

        outData.Item2 = " ";

        offsetSize += 4;
        int stringLenght = BitConverter.ToInt32(message, offsetSize);

        offsetSize += 4;
        for (int i = 0; i < stringLenght; i++)
        {
            outData.Item2 += (char)message[offsetSize + i];
        }

        return outData;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.Console;
    }

    public override byte[] Serialize(int newPlayer)
    {
        List<byte> outData = new List<byte>();

        BasicSerialize(outData, GetMessageType(), newPlayer, GetMessageFlag());

        outData.AddRange(BitConverter.GetBytes(data.Item1));
        outData.AddRange(BitConverter.GetBytes(data.Item2.Length));

        for (int i = 0; i < data.Item2.Length; i++)
        {
            outData.Add((byte)data.Item2[i]);
        }

        StartChecksum(outData);

        return outData.ToArray();
    }
}
public class NetSameName : OrderMessage<int>
{
    public override MessageType ReadMsgID(byte[] message)
    {
        return (MessageType)BitConverter.ToUInt32(message);
    }

    public override MessageFlags GetMessageFlag()
    {
        return MessageFlags.Important;
    }

    public override int Deserialize(byte[] message)
    {
        int outData;

        offsetSize = 16;

        outData = BitConverter.ToInt32(message, offsetSize);

        return outData;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.SameName;
    }

    public override byte[] Serialize(int newPlayer)
    {
        List<byte> outData = new List<byte>();

        BasicSerialize(outData, GetMessageType(), newPlayer, GetMessageFlag());

        return outData.ToArray();
    }
}
public class NetMaxPlayers : OrderMessage<int>
{
    public override MessageType ReadMsgID(byte[] message)
    {
        return (MessageType)BitConverter.ToUInt32(message);
    }

    public override MessageFlags GetMessageFlag()
    {
        return MessageFlags.Important;
    }

    public override int Deserialize(byte[] message)
    {
        int outData;

        offsetSize = 16;

        outData = BitConverter.ToInt32(message, offsetSize);

        return outData;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.MaxPlayers;
    }

    public override byte[] Serialize(int newPlayer)
    {
        List<byte> outData = new List<byte>();

        BasicSerialize(outData, GetMessageType(), newPlayer, GetMessageFlag());

        return outData.ToArray();
    }
}
public class NetTimer : OrderMessage<float>
{
    public override MessageType ReadMsgID(byte[] message)
    {
        return (MessageType)BitConverter.ToUInt32(message);
    }

    public override MessageFlags GetMessageFlag()
    {
        return MessageFlags.Ordenable;
    }

    public override float Deserialize(byte[] message)
    {
        float outData;

        offsetSize = 16;

        outData = BitConverter.ToSingle(message, offsetSize);

        return outData;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.Timer;
    }

    public override byte[] Serialize(int newPlayer)
    {
        List<byte> outData = new List<byte>();

        BasicSerialize(outData, GetMessageType(), newPlayer, GetMessageFlag());

        outData.AddRange(BitConverter.GetBytes(data));

        StartChecksum(outData);

        return outData.ToArray();
    }
}
public static class NetByteTranslator
{
    public static MessageType GetNetworkType(byte[] data)
    {
        if (data != null && data.Length > 0)
        {
            int dataOut = BitConverter.ToInt32(data, 0);
            return (MessageType)dataOut;
        }

        return 0;
    }

    public static int GetPlayerID(byte[] data)
    {
        if (data != null && data.Length > 4)
        {
            int dataOut = BitConverter.ToInt32(data, 4);
            return dataOut;
        }

        return -15;
    }

    public static MessageFlags GetFlags(byte[] data)
    {
        if (data != null && data.Length > 8)
        {
            MessageFlags dataOut = (MessageFlags)BitConverter.ToInt32(data, 8);
            return dataOut;
        }

        return 0;
    }

    public static ulong GetMesaggeID(byte[] data)
    {
        if (data != null && data.Length > 12)
        {
            ulong dataOut = BitConverter.ToUInt64(data, 12);
            return dataOut;
        }

        return 0;
    }

    public static ulong GetObjectID(byte[] data)
    {
        ulong dataOut = BitConverter.ToUInt64(data, 20);
        return dataOut;
    }
}