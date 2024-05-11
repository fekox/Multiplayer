﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using UnityEngine;

public class Client
{
    public string clientName;
    public float timeStamp;
    public int id;

    public float timer;
    public bool startTimer;
    public IPEndPoint ipEndPoint;

    public Client(IPEndPoint ipEndPoint, int id, float timeStamp)
    {
        clientName = "";
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;
        timer = 0;
        startTimer = true;
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

    public void SetStartTimer(bool newBool)
    {
        startTimer = newBool;
    }

    public void SetTimer(float playerTimer)
    {
        timer = playerTimer;
    }

    public void UpdateTimer()
    {
        timer += Time.deltaTime;

        Debug.Log("Time to disconect: " + timer + " || " + NetworkManager.Instance.TimeOut);
    }

    public void ResetTimer() 
    {
        timer = 0;
    }
}

public struct Player
{
    public string clientId;
    public int ID;

    public Player (int id, string name) 
    {
        this.ID = id;
        this.clientId = name;
    }

    public string GetPlayerName() 
    {
        return clientId;
    }
}

public class NetworkManager : MonoBehaviourSingleton<NetworkManager>, IReceiveData
{
    public IPAddress ipAddress
    {
        get; private set;
    }

    public int port
    {
        get; private set;
    }

    public bool isServer
    {
        get; private set;
    }

    public int TimeOut = 20;

    public Action<byte[], IPEndPoint> OnReceiveEvent;

    private UdpConnection connection;

    public readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();
    public List<Player> playerList = new List<Player>();
    public Player player;
    public int clientID = 0;

    private NetToClientHandShake netToClientHandShake = new NetToClientHandShake();
    private NetToServerHandShake netToSeverHandShake = new NetToServerHandShake();
    private NetConsole netConsole = new NetConsole();
    private NetPingPong netPingPong = new NetPingPong();

    private void Update()
    {
        if (connection != null)
            connection.FlushReceiveData();

        if (NetworkManager.Instance.isServer) 
        {
            if (playerList.Count > 0)
            {
                foreach (var client in clients)
                {
                    client.Value.UpdateTimer();

                    if (client.Value.timer > TimeOut)
                    {
                        RemoveClient(client.Value.GetIp());
                    }
                }
            }
        }
    }

    public void StartServer(int port)
    {
        isServer = true;
        this.port = port;
        connection = new UdpConnection(port, this);
    }

    public void StartClient(string clientName, IPAddress ip, int port)
    {
        isServer = false;

        this.port = port;
        this.ipAddress = ip;

        connection = new UdpConnection(ip, port, this);

        player = new Player(0, clientName);

        MessageManager.Instance.OnSendServerHandShake(player.ID, player.clientId);

        MessageManager.Instance.StartPingPong();
    }

    void AddClient(IPEndPoint ip)
    {
        if (!ipToId.ContainsKey(ip))
        {
            Debug.Log("Client IP: " + ip.Address);

            int id = clientID;

            ipToId[ip] = clientID;

            clients.Add(clientID, new Client(ip, id, Time.realtimeSinceStartup));
        }
    }

    public void AddPlayer(Player player) 
    {
        playerList.Add(player);
    }

    void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            Debug.Log("Removing client: " + ip.Address);

            clients.Remove(ipToId[ip]);
        }
    }

    public void OnReceiveData(byte[] data, IPEndPoint ipEndpoint)
    {
        AddClient(ipEndpoint);

        OnRecieveMessage(data, ipEndpoint);

        if (OnReceiveEvent != null)
            OnReceiveEvent.Invoke(data, ipEndpoint);
    }

    public void SendToServer(byte[] data)
    {
        connection.Send(data);
    }

    public void SendToClient(byte[] data, IPEndPoint ip) 
    {
        connection.Send(data, ip);
    }

    public void Broadcast(byte[] data)
    {
        using (var iterator = clients.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                connection.Send(data, iterator.Current.Value.ipEndPoint);
            }
        }
    }

    public MessageType OnRecieveMessage(byte[] data, IPEndPoint Ip)
    {
        MessageType typeMessage = (MessageType)BitConverter.ToInt32(data, 0);

        switch (typeMessage)
        {
            case MessageType.ToServerHandShake:

                Player newPlayer = new Player(netToSeverHandShake.Deserialize(data).Item1, netToSeverHandShake.Deserialize(data).Item2);

                newPlayer.ID = clientID;
                newPlayer.clientId = netToSeverHandShake.Deserialize(data).Item2;

                AddPlayer(newPlayer);

                netToClientHandShake.data = playerList;

                data = netToClientHandShake.Serialize();

                clientID++;
                Debug.Log("Add new client = Client Id: " + netToClientHandShake.data[netToClientHandShake.data.Count - 1].clientId + " - Id: " + netToClientHandShake.data[netToClientHandShake.data.Count - 1].ID);

                break;

            case MessageType.ToClientHandShake:

                playerList = netToClientHandShake.Deserialize(data);

                for (int i = 0; i < playerList.Count; i++)
                {
                    if (playerList[i].clientId == player.clientId)
                    {
                        player.ID = playerList[i].ID;
                        break;
                    }
                }

                break;

            case MessageType.PingPong:

                if (isServer)
                {
                    //Reseteo
                    clients[ipToId[Ip]].ResetTimer();
                }

                else
                {
                    //
                }

                break;

            case MessageType.Console:

                string playerName = "";

                for (int i = 0; i < playerList.Count; i++)
                {
                    if (playerList[i].ID == netConsole.Deserialize(data).Item1)
                    {
                        playerName = playerList[i].clientId;
                        break;
                    }
                }

                ChatScreen.Instance.OnReceiveDataEvent(playerName + ": " + netConsole.Deserialize(data).Item2);
                clients[ipToId[Ip]].ResetTimer();

                break;

            case MessageType.Position:

                break;

            default:

                Debug.LogError("Message type not found");

                break;
        }

        if (isServer)
        {
            Broadcast(data);
        }

        return typeMessage;
    }
}
