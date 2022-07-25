using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UI_ChatInputField : MonoBehaviour
{
    public InputField m_UI_ChatInputField;
    public UI_ChatWindow m_UI_ChatWindow;

    private bool m_WritingChat = false;

    public void SendChatMessage(string message) {
        if (message == string.Empty) {
            return;
        }
        if (!Client.instance.IsClientReady()) {
            return;
        }
        int fromId = Client.instance.myId;
        m_UI_ChatWindow.PushTextMessage(fromId, message);
        ChatClientSend.SendChatMessage(fromId, message);
    }

    public void ToggleWritingChat() {
        if (m_WritingChat) {
            SendChatMessage(m_UI_ChatInputField.text);
        }
        m_WritingChat = !m_WritingChat;
        m_UI_ChatInputField.text = string.Empty;
        m_UI_ChatInputField.gameObject.SetActive(m_WritingChat);
        m_UI_ChatInputField.ActivateInputField();
    }

    public void WritingChatOff() {
        m_WritingChat = false;
        m_UI_ChatInputField.text = string.Empty;
        m_UI_ChatInputField.gameObject.SetActive(false);
    }

    public bool GetWritingChat() {
        return m_WritingChat;
    }
}
