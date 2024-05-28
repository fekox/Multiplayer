using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourSingleton<NetworkManager>, IReceiveData
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [SerializeField] private GameObject latencyGO;

    [SerializeField] private GameObject ErrorPopup;

    [SerializeField] private GameObject MaxPlayerPopup;

    [SerializeField] private GameObject chatScreen;

    [SerializeField] private TextMeshProUGUI latencyText;

    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Setup")]
    [SerializeField] private string menuName = "Menu";

    [SerializeField] private int maxPlayersInGame = 4;

    private NetToClientHandShake netToClientHandShake = new NetToClientHandShake();
    private NetToServerHandShake netToSeverHandShake = new NetToServerHandShake();
    private NetPingPong netPingPong = new NetPingPong();
    private NetConsole netConsole = new NetConsole();
    private NetVector2 netVector2 = new NetVector2();
    private NetSameName netSameName = new NetSameName();
    private NetMaxPlayers netMaxPlayers = new NetMaxPlayers();

    private UdpConnection connection;

    private DateTime serverTimer = DateTime.UtcNow;

    private int clientID = 0;

    private bool sameName = false;
    private bool maxPlayers = false;
    private bool startGame = false;


    [Header("Game Timer")]
    public float timerSeg = 240;

    private int seconds;
    private int minutes;

    public bool initialized = false;

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

    public int TimeOut = 10;

    [Header("Players List")]
    public List<Player> playerList = new List<Player>();

    private Player playerData;

    [Header("Clients")]
    public Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();

    public Action<byte[], IPEndPoint> OnReceiveEvent;

    private void Update()
    {
        if (connection != null)
            connection.FlushReceiveData();

        if (isServer)
        {
            StartClientTimer();
        }

        if (!isServer && initialized)
        {
            StartServerTimer();
        }

        if(playerList.Count > 1)
        {
            startGame = true;
        }

        if (startGame)
        {
            int oneMinute = 60;

            timerSeg -= Time.deltaTime;

            seconds = (int)timerSeg % oneMinute;
            minutes = (int)timerSeg / oneMinute;

            timerText.text = string.Format("{00:00}:{1:00}", minutes, seconds);
        }
    }

    public void StartServer(int port)
    {
        isServer = true;

        this.port = port;
        connection = new UdpConnection(port, this);
        latencyGO.SetActive(true);
    }

    public void StartServerTimer()
    {
        latencyText.text = "Latency: " + (DateTime.UtcNow - serverTimer).Seconds;

        if ((DateTime.UtcNow - serverTimer).Seconds > TimeOut)
        {
            Disconect();
            SceneManager.LoadScene(menuName);
        }
    }

    public void ResetServerTimer()
    {
        serverTimer = DateTime.UtcNow;
    }

    public void StartClient(string clientName, IPAddress ip, int port)
    {
        isServer = false;

        this.port = port;
        this.ipAddress = ip;

        connection = new UdpConnection(ip, port, this);

        playerData = new Player(0, clientName);

        latencyGO.SetActive(true);

        OnSendServerHandShake(playerData.ID, playerData.tagName);

        StartPing();
    }

    public void StartClientTimer() 
    {
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i].connected == true)
            {
                if ((DateTime.UtcNow - clients[i].timer).Seconds > TimeOut)
                {
                    RemoveClient(clients[i].ipEndPoint);
                }
            }
        }            
    }

    public void ResetClientTimer(int PlayerId)
    {
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i].id == PlayerId)
            {
                clients[i].resetTimer();
            }
        }
    }

    void AddClient(IPEndPoint ip, string name)
    {
        if (!ipToId.ContainsKey(ip))
        {
            Debug.Log("Client IP: " + ip.Address);

            int id = clientID;

            ipToId[ip] = clientID;

            clients.Add(clientID, new Client(ip, id, Time.realtimeSinceStartup));
            AddPlayer(new Player(clientID, name));

            clientID++;
        }
    }

    public void AddPlayer(Player player)
    {
        playerList.Add(player);
    }

    public void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            clients.ToArray()[ipToId[ip]].Value.connected = false;

            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList.ToArray()[i].ID == ipToId[ip])
                {
                    playerList.Remove(playerList.ToArray()[i]);
                }
            }

            SendNewListOfPlayers();
        }
    }

    public void Disconect() 
    {
        clients.Clear();
        playerList.Clear();
        initialized = false;
    }

    public void OnReceiveData(byte[] data, IPEndPoint ipEndpoint)
    {
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

    public bool CheckAlreadyUseName(string newPlayerName)
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].tagName == newPlayerName)
            {
                return true;
            }
        }

        return false;
    }

    public bool CheckMaxPlayers(int currentPlayers) 
    {
        if(maxPlayersInGame == currentPlayers) 
        {
            return true;
        }

        else 
        {
            return false;
        }
    }

    private void CheckMessage(byte[] message)
    {
        if (isServer)
        {
            Broadcast(message);
        }

        else
        {
            SendToServer(message);
        }
    }

    public void OnSendConsoleMessage(string message)
    {
        netConsole.data.Item1 = playerData.ID;
        netConsole.data.Item2 = message;

        Debug.Log(netConsole.data.Item1);

        if (isServer)
        {
            ChatScreen.Instance.OnReceiveDataEvent(message);
        }

        CheckMessage(netConsole.Serialize());
    }

    public void OnSendServerHandShake(int id, string name)
    {
        netToSeverHandShake.data.Item1 = id;
        netToSeverHandShake.data.Item2 = name;
        SendToServer(netToSeverHandShake.Serialize());
    }

    public void SendNewListOfPlayers()
    {
       netToClientHandShake.data = playerList;

        byte[] data = netToClientHandShake.Serialize();

        Broadcast(data);
    }
    public void StartPing()
    {
        initialized = true;
        ResetServerTimer();

        netPingPong.data = playerData.ID;

        SendToServer(netPingPong.Serialize());
    }

    public void StartPong(byte[] data, IPEndPoint IP)
    {
        ResetClientTimer(netPingPong.Deserialize(data));
        SendToClient(netPingPong.Serialize(), IP);
    }

    public MessageType OnRecieveMessage(byte[] data, IPEndPoint Ip)
    {
        MessageType typeMessage = (MessageType)BitConverter.ToInt32(data, 0);

        switch (typeMessage)
        {
            case MessageType.ToServerHandShake:

                (int, string) info = netToSeverHandShake.Deserialize(data);

                if (CheckAlreadyUseName(info.Item2))
                {
                    data = netSameName.Serialize();

                    SendToClient(data, Ip);

                    sameName = true;
                }

                netToClientHandShake.data = playerList;

                if (CheckMaxPlayers(playerList.Count))
                {
                    data = netMaxPlayers.Serialize();

                    SendToClient(data, Ip);

                    maxPlayers = true;
                }

                else 
                {
                    AddClient(Ip, info.Item2);

                    netToClientHandShake.data = playerList;

                    data = netToClientHandShake.Serialize();

                    gameManager.SpawnPlayer(info.Item2);

                    Debug.Log("add new client = Client Id: " + netToClientHandShake.data[netToClientHandShake.data.Count - 1].tagName + " - Id: " + netToClientHandShake.data[netToClientHandShake.data.Count - 1].ID);
                    
                    maxPlayers = false;
                    sameName = false;
                }

                break;

            case MessageType.ToClientHandShake:

                playerList = netToClientHandShake.Deserialize(data);                

                for (int i = 0; i < playerList.Count; i++)
                {
                    if (playerList[i].tagName == playerData.tagName)
                    {
                        playerData.ID = playerList[i].ID;
                        break;
                    }
                }

                maxPlayers = false;
                sameName = false;
                break;

            case MessageType.PingPong:

                if (netPingPong.IsChecksumOk(data))
                {
                    if (isServer) 
                    {
                        StartPong(data, Ip);
                    }

                    else 
                    {
                        StartPing();
                    }

                    Debug.Log(nameof(NetPingPong) + ": message is ok.");
                }

                else 
                {
                    Debug.Log(nameof(NetPingPong) + ": message is corrupt.");
                }

                sameName = false;
                maxPlayers = false;
                break;

            case MessageType.Console:

                if (netConsole.IsChecksumOk(data))
                {
                    string playerName = "";

                    for (int i = 0; i < playerList.Count; i++)
                    {
                        if (playerList[i].ID == netConsole.Deserialize(data).Item1)
                        {
                            playerName = playerList[i].tagName;
                            break;
                        }
                    }

                    ChatScreen.Instance.OnReceiveDataEvent(playerName + ": " + netConsole.Deserialize(data).Item2);

                    Debug.Log(nameof(NetConsole) + ": message is ok.");
                }

                else
                {
                    Debug.Log(nameof(NetConsole) + ": message is corrupt.");
                }

                sameName = false;
                maxPlayers = false;
                break;

            case MessageType.Position:

                if(netVector2.IsChecksumOk(data)) 
                {
                    (int, Vector2) infoPos = netVector2.Deserialize(data);

                    for (int i = 0; i < playerList.Count; i++)
                    {
                        if (playerList[i].ID == infoPos.Item1)
                        {
                            playerList[i].position = infoPos.Item2;


                        }
                    }
                }

                else 
                {
                
                }

                sameName = false;
                maxPlayers = false;
                break;

            case MessageType.SameName:

                ErrorPopup.SetActive(true);
                chatScreen.SetActive(false);

                break;

            case MessageType.MaxPlayers:

                MaxPlayerPopup.SetActive(true);
                chatScreen.SetActive(false);

                break;

            default:

                Debug.LogError("Message type not found");

                break;
        }

        if (isServer && !sameName && !maxPlayers)
        {
            Broadcast(data);
        }

        return typeMessage;
    }
}