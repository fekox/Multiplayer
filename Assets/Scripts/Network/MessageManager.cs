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

    public MessageType ReadMessage(MessageType messageType) 
    {
        switch (messageType)
        {
            case MessageType.ClientToServerHandShake:

                break;

            case MessageType.ServerToClientHandShake:

                break;

            case MessageType.Console:
                break;

            case MessageType.Position:
                break;
        }

        return messageType;
    }
}
