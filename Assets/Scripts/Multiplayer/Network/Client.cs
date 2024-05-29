using System;
using System.Net;
using UnityEngine;

public class Client
{
    public string clientName;
    public float timeStamp;
    public int id;

    public DateTime timer = DateTime.UtcNow;
    public bool connected;
    public IPEndPoint ipEndPoint;

    public Client(IPEndPoint ipEndPoint, int id, float timeStamp)
    {
        clientName = "";
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;

        this.connected = true;
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

    public string GetClientName()
    {
        return clientName;
    }

    public void resetTimer() 
    {
        this.timer = DateTime.UtcNow;
    }
}
