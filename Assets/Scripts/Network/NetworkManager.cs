using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public struct Client
{
    public string clientName;
    public float timeStamp;
    public int id;
    public IPEndPoint ipEndPoint;

    public Client(string clientName, IPEndPoint ipEndPoint, int id, float timeStamp)
    {
        this.clientName = clientName;
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;
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
}

public class NetworkManager : MonoBehaviourSingleton<NetworkManager>, IReceiveData
{
    Client client;
    ChatScreen chatScreen;

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

    public int TimeOut = 30;

    public Action<string, byte[], IPEndPoint> OnReceiveEvent;

    private UdpConnection connection;

    private readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
    private readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();

    public void StartServer(int port)
    {
        isServer = true;
        this.port = port;
        connection = new UdpConnection(port, this);
    }

    public void StartClient(string clientName, IPAddress ip, int port)
    {
        isServer = false;

        client.clientName = clientName;
        this.port = port;
        this.ipAddress = ip;

        connection = new UdpConnection(ip, port, this);

        AddClient(clientName, new IPEndPoint(ip, port));
    }

    void AddClient(string clientName, IPEndPoint ip)
    {
        if (!ipToId.ContainsKey(ip))
        {
            Debug.Log("Adding client: Name:" + clientName + " || IP:" + client.id + " || IP:" + ip.Address);

            ipToId[ip] = client.id;

            clients.Add(client.id, new Client(clientName, ip, client.id, Time.realtimeSinceStartup));

            client.id++;

            client.SetClientID(client.id);
        }
    }

    void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            Debug.Log("Removing client: " + ip.Address);
            clients.Remove(ipToId[ip]);

            client.SetClientID(client.id);
        }
    }

    public void OnReceiveData(string clientName, byte[] data, IPEndPoint ipEndpoint)
    {
        AddClient(client.clientName, ipEndpoint);

        if (OnReceiveEvent != null)
            OnReceiveEvent.Invoke(clientName, data, ipEndpoint);
    }

    public void SendToServer(byte[] data)
    {
        connection.Send(data);
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

    void Update()
    {
        // Flush the data in main thread
        if (connection != null)
            connection.FlushReceiveData();
    }
}
