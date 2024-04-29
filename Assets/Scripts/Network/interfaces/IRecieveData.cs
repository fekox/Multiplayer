using System.Net;
using UnityEditor;

public interface IReceiveData
{
    void OnReceiveData(string clientName, byte[] data, IPEndPoint ipEndpoint);
}