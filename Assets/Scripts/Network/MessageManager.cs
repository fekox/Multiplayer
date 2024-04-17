using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    public void ClientOrServerCheck()
    {
        if (NetworkManager.Instance.isServer)
        {

        }

        else
        {

        }
    }

    public void ReadMessage(MessageType messageType) 
    {
        switch (messageType) 
        {
            case MessageType.HandShake:
            
            break;

            case MessageType.Position:

            break;

            case MessageType.String:

            break;

            case MessageType.Console:

            break;
        }
    }
}
