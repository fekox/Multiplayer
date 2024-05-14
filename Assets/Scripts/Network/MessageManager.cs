using System;
using System.Net;
using UnityEngine;

public class MessageManager : MonoBehaviourSingleton<MessageManager>
{
    private NetConsole netConsole = new NetConsole();
    private NetToServerHandShake netToSeverHandShake = new NetToServerHandShake();
    private NetPingPong netPingPong = new NetPingPong();

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
        netToSeverHandShake.data.ID = id;
        netToSeverHandShake.data.clientId = name;
        NetworkManager.Instance.SendToServer(netToSeverHandShake.Serialize());
    }

    public void StartPing()
    {
        NetworkManager.Instance.SendToServer(netPingPong.Serialize());
    }

    public void StartPong(IPEndPoint IP) 
    {
        NetworkManager.Instance.SendToClient(netPingPong.Serialize(), IP);
    }
}
