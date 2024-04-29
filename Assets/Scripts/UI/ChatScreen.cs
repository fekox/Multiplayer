using System;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;



public class ChatScreen : MonoBehaviourSingleton<ChatScreen>
{
    public Text messages;
    public InputField inputMessage;
    public NetString netString;
    public NetClientToServerHandShake netClientToServerHandShake;
    public NetServerToClientHandShake netServerToClientHandShake;


    protected override void Initialize()
    {
        inputMessage.onEndEdit.AddListener(OnEndEdit);

        this.gameObject.SetActive(false);

        NetworkManager.Instance.OnReceiveEvent += OnReceiveDataEvent;
    }

    void OnReceiveDataEvent(string clientName, byte[] data, IPEndPoint ep)
    {
        MessageType type = (MessageType)BitConverter.ToUInt32(data);

        if (NetworkManager.Instance.isServer)
        {
            switch (type)
            {
                case MessageType.ClientToServerHandShake:

                    NetworkManager.Instance.Broadcast(data);

                break;

                case MessageType.ServerToClientHandShake:

                    NetworkManager.Instance.Broadcast(data);

                break;

                case MessageType.Console:
                break;

                case MessageType.Position:
                break;

                case MessageType.String:

                    NetworkManager.Instance.Broadcast(data);

                break;
            }
        }

        else 
        {
            switch (type)
            {
                case MessageType.ClientToServerHandShake:

                    messages.text += netClientToServerHandShake.Deserialize(data) + System.Environment.NewLine;

                break;

                case MessageType.ServerToClientHandShake:

                    messages.text += netServerToClientHandShake.Deserialize(data) + System.Environment.NewLine;

                break;

                case MessageType.Console:
                break;

                case MessageType.Position:
                break;

                case MessageType.String:

                    messages.text += netString.Deserialize(data) + System.Environment.NewLine;

                break;
            }
        }
    }

    void OnEndEdit(string str)
    {
        if (inputMessage.text != "")
        {
            netString = new NetString(str);

            if (NetworkManager.Instance.isServer)
            {
                NetworkManager.Instance.Broadcast(netString.Serialize());
                messages.text += inputMessage.text + System.Environment.NewLine;
            }

            else
            {
                NetworkManager.Instance.SendToServer(netString.Serialize());
            }

            inputMessage.ActivateInputField();
            inputMessage.Select();
            inputMessage.text = "";
        }
    }
}