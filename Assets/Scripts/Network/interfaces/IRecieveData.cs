using System.Net;
using UnityEditor;

public interface IReceiveData
{
    void OnReceiveData(byte[] data, IPEndPoint ipEndpoint);
}