using System;
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

        Debug.Log("Time to disconect player: " + timer + " || " + NetworkManager.Instance.TimeOut);
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

    public Player(int id, string name)
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
    public float serverTimer = 0;

    public Action<byte[], IPEndPoint> OnReceiveEvent;

    private UdpConnection connection;

    public readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();

    private NetToClientHandShake netToClientHandShake = new NetToClientHandShake();
    private NetToServerHandShake netToSeverHandShake = new NetToServerHandShake();
    private NetPingPong netPingPong = new NetPingPong();
    private NetConsole netConsole = new NetConsole();
    private NetVector3 netVector3 = new NetVector3();

    public List<Player> playerList = new List<Player>();
    public Player player;
    public int clientID = 0;

    private void Update()
    {
        if (connection != null)
            connection.FlushReceiveData();

        if (isServer)
        {
            StartClientTimer();
        }

        else
        {
            StartServerTimer();
        }
    }

    public void StartServer(int port)
    {
        isServer = true;
        this.port = port;
        connection = new UdpConnection(port, this);
    }

    public void StartServerTimer()
    {
        if (playerList.Count > 0)
        {
            if (serverTimer < TimeOut)
            {
                serverTimer += Time.deltaTime;

                Debug.Log("Time to close server: " + serverTimer + " || " + TimeOut);

                if (serverTimer > TimeOut)
                {
                    Debug.Log("Close Server");
                    connection.DisposeAndClose();
                }
            }
        }
    }

    public void ResetServerTimer()
    {
        serverTimer = 0;
    }

    public void StartClient(string clientName, IPAddress ip, int port)
    {
        isServer = false;

        this.port = port;
        this.ipAddress = ip;

        connection = new UdpConnection(ip, port, this);

        player = new Player(0, clientName);

        MessageManager.Instance.OnSendServerHandShake(player.ID, player.clientId);

        MessageManager.Instance.StartPing();
    }

    public void StartClientTimer() 
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

                //if (netToSeverHandShake.IsChecksumOk(data)) 
                //{
                //    player = new Player(netToSeverHandShake.Deserialize(data).ID, netToSeverHandShake.Deserialize(data).clientId);

                //    player.ID = clientID;
                //    player.clientId = netToSeverHandShake.Deserialize(data).clientId;

                //    AddPlayer(player);

                //    netToClientHandShake.data = playerList;

                //    data = netToClientHandShake.Serialize();

                //    clientID++;

                //    Debug.Log(nameof(NetToServerHandShake) + ": message is ok.");
                //}

                //else 
                //{
                //    Debug.Log(nameof(NetToServerHandShake) + ": message is corrupt.");
                //}

                player = new Player(netToSeverHandShake.Deserialize(data).ID, netToSeverHandShake.Deserialize(data).clientId);

                player.ID = clientID;
                player.clientId = netToSeverHandShake.Deserialize(data).clientId;

                AddPlayer(player);

                netToClientHandShake.data = playerList;

                data = netToClientHandShake.Serialize();

                clientID++;

                break;

            case MessageType.ToClientHandShake:

                //if (netToClientHandShake.IsChecksumOk(data)) 
                //{
                //    playerList = netToClientHandShake.Deserialize(data);

                //    for (int i = 0; i < playerList.Count; i++)
                //    {
                //        if (playerList[i].clientId == player.clientId)
                //        {
                //            player.ID = playerList[i].ID;
                //            break;
                //        }
                //    }

                //    Debug.Log(nameof(NetToClientHandShake) + ": message is ok.");
                //}

                //else 
                //{
                //    Debug.Log(nameof(NetToClientHandShake) + ": message is corrupt.");
                //}

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

                if (netPingPong.IsChecksumOk(data))
                {
                    if (isServer)
                    {
                        clients[ipToId[Ip]].ResetTimer();
                        MessageManager.Instance.StartPong(Ip);
                    }

                    else
                    {
                        ResetServerTimer();
                        MessageManager.Instance.StartPing();
                    }

                    Debug.Log(nameof(NetPingPong) + ": message is ok.");
                }

                else 
                {
                    Debug.Log(nameof(NetPingPong) + ": message is corrupt.");
                }

                break;

            case MessageType.Console:

                //if (netConsole.IsChecksumOk(data))
                //{
                //    string playerName = "";

                //    for (int i = 0; i < playerList.Count; i++)
                //    {
                //        if (playerList[i].ID == netConsole.Deserialize(data).Item1)
                //        {
                //            playerName = playerList[i].clientId;
                //            break;
                //        }
                //    }

                //    ChatScreen.Instance.OnReceiveDataEvent(playerName + ": " + netConsole.Deserialize(data).Item2);
                //    clients[ipToId[Ip]].ResetTimer();

                //    Debug.Log(nameof(NetConsole) + ": message is ok.");
                //}

                //else 
                //{
                //    Debug.Log(nameof(NetConsole) + ": message is corrupt.");
                //}

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

                if(netVector3.IsChecksumOk(data)) 
                {
                
                }

                else 
                {
                
                }

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