using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Client
{
    public string clientName;
    public float timeStamp;
    public int id;

    public DateTime timer = DateTime.UtcNow;
    public bool startTimer;
    public IPEndPoint ipEndPoint;

    public Client(IPEndPoint ipEndPoint, int id, float timeStamp)
    {
        clientName = "";
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;

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

    public string GetClientName() 
    {
        return clientName;
    }

    public void ResetClientTimer() 
    {
        this.timer = DateTime.UtcNow;
    }
}

public struct Player
{
    public string tagName;
    public int ID;

    public Player(int id, string name)
    {
        this.ID = id;
        this.tagName = name;
    }

    public string GetPlayerName()
    {
        return tagName;
    }

    public int GetPlayerID() 
    {
        return ID;
    }
}

public class NetworkManager : MonoBehaviourSingleton<NetworkManager>, IReceiveData
{
    [SerializeField] private GameManager gameManager;

    [SerializeField] private TextMeshProUGUI latencyText;
    [SerializeField] private GameObject latencyGO;

    [SerializeField] private GameObject ErrorPopup;

    [SerializeField] private GameObject MaxPlayerPopup;

    [SerializeField] private GameObject chatScreen;

    [SerializeField] private string menuName = "Menu";

    private NetToClientHandShake netToClientHandShake = new NetToClientHandShake();
    private NetToServerHandShake netToSeverHandShake = new NetToServerHandShake();
    private NetPingPong netPingPong = new NetPingPong();
    private NetConsole netConsole = new NetConsole();
    private NetVector3 netVector3 = new NetVector3();
    private NetSameName netSameName = new NetSameName();

    private UdpConnection connection;

    private DateTime serverTimer = DateTime.UtcNow;

    private int clientID = 0;

    private bool sameName = false;

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

    public Action<byte[], IPEndPoint> OnReceiveEvent;

    public readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();

    public Player playerData;
    public List<Player> playerList;

    private void Start()
    {
        playerList = new List<Player>();
    }

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

        if(playerList.Count > 0)
        {
            gameManager.StartGame();
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
        if (playerList.Count > 0)
        {
            latencyText.text = "Latency: " + (DateTime.UtcNow - serverTimer).Seconds;

            if ((DateTime.UtcNow - serverTimer).Seconds > TimeOut)
            {
                Debug.Log("Close Server");
                connection.DisposeAndClose();
                SceneManager.LoadScene(menuName);
            }
        }
    }

    public void StartClient(string clientName, IPAddress ip, int port)
    {
        isServer = false;

        this.port = port;
        this.ipAddress = ip;

        connection = new UdpConnection(ip, port, this);

        playerData = new Player(0, clientName);

        latencyGO.SetActive(true);

        MessageManager.Instance.OnSendServerHandShake(playerData.ID, playerData.tagName);

        MessageManager.Instance.StartPing();

        gameManager.SpawnPlayer(clientName);
    }

    public void StartClientTimer() 
    {
        if (playerList.Count > 0)
        {
            foreach (var client in clients)
            {
                latencyText.text = "Latency: " + (DateTime.UtcNow - client.Value.timer).Seconds;

                if ((DateTime.UtcNow - client.Value.timer).Seconds > TimeOut) 
                {
                    RemovePlayer(playerData.ID);
                    RemoveClient(client.Value.GetIp());
                    SceneManager.LoadScene(menuName);
                }
            }
        }
    }

    public void ResetServerTimer() 
    {
        serverTimer = DateTime.UtcNow;
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

    public void RemovePlayer(int playerID) 
    {
        for (int i = 0; i < playerList.Count; i++) 
        {
            if (playerID == playerList[i].ID)
            {
                playerList.Remove(playerList[i]);
            }
        }
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
        int maxPlayers = 4;

        if(maxPlayers == currentPlayers) 
        {
            return true;
        }

        else 
        {
            return false;
        }
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

                else 
                {
                    AddClient(Ip, info.Item2);

                    netToClientHandShake.data = playerList;

                    data = netToClientHandShake.Serialize();

                    Debug.Log("add new client = Client Id: " + netToClientHandShake.data[netToClientHandShake.data.Count - 1].tagName + " - Id: " + netToClientHandShake.data[netToClientHandShake.data.Count - 1].ID);
                    sameName = false;
                }

                break;

            case MessageType.ToClientHandShake:

                playerList = netToClientHandShake.Deserialize(data);

                if(CheckMaxPlayers(playerList.Count))
                {
                    MaxPlayerPopup.SetActive(true);
                    chatScreen.SetActive(false);
                }

                else 
                {
                    for (int i = 0; i < playerList.Count; i++)
                    {
                        if (playerList[i].tagName == playerData.tagName)
                        {
                            playerData.ID = playerList[i].ID;
                            break;
                        }
                    }
                }

                sameName = false;
                break;

            case MessageType.PingPong:

                if (netPingPong.IsChecksumOk(data))
                {
                    if (isServer) 
                    {
                        clients[ipToId[Ip]].ResetClientTimer();
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

                sameName = false;
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
                break;

            case MessageType.Position:

                if(netVector3.IsChecksumOk(data)) 
                {
                
                }

                else 
                {
                
                }

                break;

            case MessageType.SameName:

                ErrorPopup.SetActive(true);
                chatScreen.SetActive(false);

                break;

            default:

                Debug.LogError("Message type not found");

                break;
        }

        if (isServer && !sameName)
        {
            Broadcast(data);
        }

        return typeMessage;
    }
}