using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

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

                newPlayer.ID = NetworkManager.Instance.;
                newPlayer.clientId = netMessageToServer.Deseria1lize(data).Item2;

                NetworkManager.Instance.addPlayer(newPlayer);

                netMessageToClient.data = NetworkManager.Instance.players;

                data = netMessageToClient.Serialize();

                NetworkManager.Instance.clientId++;
                Debug.Log("add new client = Client Id: " + netMessageToClient.data[netMessageToClient.data.Count - 1].clientId + " - Id: " + netMessageToClient.data[netMessageToClient.data.Count - 1].id);

                break;

            case MessageType.ToClientHandShake:

                NetworkManager.Instance.players = netMessageToClient.Deserialize(data);
                for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
                {
                    if (NetworkManager.Instance.players[i].clientId == NetworkManager.Instance.playerData.clientId)
                    {
                        NetworkManager.Instance.playerData.id = NetworkManager.Instance.players[i].id;
                        break;
                    }
                }

                break;

            case MessageType.Console:

                string playerName = "";
                for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
                {
                    if (NetworkManager.Instance.players[i].id == netCode.Deserialize(data).Item1)
                    {
                        playerName = NetworkManager.Instance.players[i].clientId;
                        break;
                    }
                }

                ChatScreen.Instance.OnReceiveDataEvent(playerName + " : " + netCode.Deserialize(data).Item2);
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

        return messageType;
    }

    public void CheckMessage(byte[] message)
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
