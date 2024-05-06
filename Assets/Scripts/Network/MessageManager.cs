using System;
using System.Net;
using UnityEngine;

public class MessageManager : MonoBehaviourSingleton<MessageManager>
{
    private NetConsole netConsole = new NetConsole();
    private NetToClientHandShake netToClientHandShake = new NetToClientHandShake();
    private NetToServerHandShake netToSeverHandShake =  new NetToServerHandShake();

    public MessageType OnRecieveMessage(byte[] data, IPEndPoint Ip) 
    {
        MessageType typeMessage = (MessageType)BitConverter.ToInt32(data, 0);

        switch (typeMessage)
        {
            case MessageType.ToServerHandShake:

                Player newPlayer = new Player(netToSeverHandShake.Deserialize(data).Item1, netToSeverHandShake.Deserialize(data).Item2);

                newPlayer.ID = NetworkManager.Instance.clientID;
                newPlayer.clientId = netToSeverHandShake.Deserialize(data).Item2;

                NetworkManager.Instance.AddPlayer(newPlayer);

                netToClientHandShake.data = NetworkManager.Instance.playerList;

                data = netToClientHandShake.Serialize();

                NetworkManager.Instance.clientID++;
                Debug.Log("Add new client = Client Id: " + netToClientHandShake.data[netToClientHandShake.data.Count - 1].clientId + " - Id: " + netToClientHandShake.data[netToClientHandShake.data.Count - 1].ID);

            break;

            case MessageType.ToClientHandShake:

                NetworkManager.Instance.playerList = netToClientHandShake.Deserialize(data);

                for (int i = 0; i < NetworkManager.Instance.playerList.Count; i++)
                {
                    if (NetworkManager.Instance.playerList[i].clientId == NetworkManager.Instance.player.clientId)
                    {
                        NetworkManager.Instance.player.ID = NetworkManager.Instance.playerList[i].ID;
                        break;
                    }
                }

            break;

            case MessageType.Console:

                string playerName = "";

                for (int i = 0; i < NetworkManager.Instance.playerList.Count; i++)
                {
                    if (NetworkManager.Instance.playerList[i].ID == netConsole.Deserialize(data).Item1)
                    {
                        playerName = NetworkManager.Instance.playerList[i].clientId;
                        break;
                    }
                }

                ChatScreen.Instance.OnReceiveDataEvent(playerName + ": " + netConsole.Deserialize(data).Item2);

            break;

            case MessageType.Position:

            break;

            default:

                Debug.LogError("Message type not found");

            break;
        }

        if (NetworkManager.Instance.isServer)
        {
            NetworkManager.Instance.Broadcast(data);
        }

        return typeMessage;
    }

    private void CheckMessage(byte[] message)
    {
        if (NetworkManager.Instance.isServer)
        {
            NetworkManager.Instance.Broadcast(message);
        }

        else
        {
            NetworkManager.Instance.SendToServer(message);
        }
    }

    public void OnSendConsoleMessage(string message) 
    {
        netConsole.data.Item1 = NetworkManager.Instance.player.ID;
        netConsole.data.Item2 = message;

        if (NetworkManager.Instance.isServer)
        {
            ChatScreen.Instance.OnReceiveDataEvent(message);
        }

        CheckMessage(netConsole.Serialize());
    }

    public void OnSendServerHandShake(int id, string name) 
    {
        netToSeverHandShake.data.Item1 = id;
        netToSeverHandShake.data.Item2 = name;
        NetworkManager.Instance.SendToServer(netToSeverHandShake.Serialize());
    }
}
