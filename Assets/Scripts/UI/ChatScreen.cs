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
    public NetConsole netConsole;

    protected override void Initialize()
    {
        inputMessage.onEndEdit.AddListener(OnEndEdit);

        this.gameObject.SetActive(false);
    }

    public void OnReceiveDataEvent(string message)
    {
        messages.text += message + System.Environment.NewLine;
    }

    void OnEndEdit(string str)
    {
        if (inputMessage.text != "")
        {
            inputMessage.ActivateInputField();
            inputMessage.Select();
            inputMessage.text = "";

            MessageManager.Instance.OnSendConsoleMessage(str);
        }
    }
}