using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine;

public class UI_ChatInputField : MonoBehaviour
{
    public InputField m_UI_ChatInputField;
    public UI_ChatWindow m_UI_ChatWindow;

    private bool m_WritingChat = false;

    public void SubmitChatMessage(string chatText) {
        if (chatText == string.Empty) {
            return;
        }
        if (!ChatClient.instance.IsConnected()) {
            return;
        }
        string message = $"<noparse>{chatText}</noparse>";
        int fromId = Client.instance.myId;
        m_UI_ChatWindow.PushTextMessage(fromId, message);
        ChatClientSend.SendChatMessage(fromId, message);
    }

    // Remove All Tag (Unused)
    private string Strip(string text) {
        return Regex.Replace(text, @"<(.|\n)*?>", string.Empty);
    }

    public void ToggleWritingChat() {
        if (m_WritingChat) {
            //string chatText = Strip(m_UI_ChatInputField.text);
            SubmitChatMessage(m_UI_ChatInputField.text);
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
