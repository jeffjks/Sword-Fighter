using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_ChatInputField : MonoBehaviour
{
    public InputField m_UI_ChatInputField;
    public UI_ChatWindow m_UI_ChatWindow;

    private PlayerInput _playerInput;

    public bool IsWritingChat {
        get;
        private set;
    }

    public void Init(PlayerInput playerInput)
    {
        _playerInput = playerInput;
    }

    public void HandleSubmitInput()
    {
        if (IsWritingChat)
        {
            if (m_UI_ChatInputField.text != string.Empty)
                SubmitChatMessage();
            CloseChatInputField();
        }
        else
        {
            OpenChatInputField();
        }
    }

    private void OpenChatInputField()
    {
        IsWritingChat = true;
        m_UI_ChatInputField.gameObject.SetActive(true);
        m_UI_ChatInputField.ActivateInputField();
        _playerInput.SwitchCurrentActionMap("InChat");
    }

    public void CloseChatInputField()
    {
        if (IsWritingChat == false)
            return;
        IsWritingChat = false;
        m_UI_ChatInputField.text = string.Empty;
        m_UI_ChatInputField.gameObject.SetActive(false);
        _playerInput.SwitchCurrentActionMap("ActionMap");
    }

    private void SubmitChatMessage()
    {
        if (IsWritingChat == false)
            return;
        SendChatMessage(m_UI_ChatInputField.text);
        CloseChatInputField();
    }

    private void SendChatMessage(string message) {
        if (message == string.Empty)
            return;
        if (!ChatClient.instance.IsConnected())
            return;
        if (!ChatClient.instance.IsReceivedWelcomeMessage())
            return;
        int fromId = Client.instance.myId;
        m_UI_ChatWindow.PushTextMessage(fromId, message);
        ChatClientSend.SendNetworkMessage(new ChatMessage(fromId, message));
    }
}
