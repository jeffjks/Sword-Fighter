using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Text;

struct MessageInfo {
    public string messageType;
    public string colorCode;

    public MessageInfo(string messageType, string colorCode) {
        this.messageType = messageType;
        this.colorCode = colorCode;
    }
};

public class UI_ChatWindow : MonoBehaviour
{
    const int ERROR_MESSAGE = -1;
    const int SERVER_MESSAGE = 127;
    const int ADMIN_MESSAGE = 126;
    
    public Text chatText;
    public ScrollRect scrollRect;
    public ContentSizeFitter contentSizeFitter;

    private StringBuilder sb = new StringBuilder();
    private int messageCount = 0;
    private Queue<int> messageLengthQueue = new Queue<int>();
    private Dictionary<int, MessageInfo> specialMessage = new Dictionary<int, MessageInfo>();

    UI_ChatWindow() {
        specialMessage[ERROR_MESSAGE] = new MessageInfo("[오류]", "#FF0000FF"); // Red
        specialMessage[SERVER_MESSAGE] = new MessageInfo("[공지]", "#DF7401FF"); // Dark Orange
        specialMessage[ADMIN_MESSAGE] = new MessageInfo("[관리자]", "#DF7401FF"); // Dark Orange
    }

    public void PushTextMessage(int fromId, string message) {
        string userName, defaultColor;
        bool isSpecialMessage = true;
        if (specialMessage.ContainsKey(fromId)) {
            userName = specialMessage[fromId].messageType;
            defaultColor = specialMessage[fromId].colorCode;
        }
        else {
            isSpecialMessage = false;
            try {
                userName = GameManager.players[fromId].username;
            }
            catch (KeyNotFoundException) {
                userName = "(Unknown)";
            }
            defaultColor = "#000000FF"; // Black
        }

        if (messageCount > 0) {
            sb.Append("\n");
        }

        if (isSpecialMessage) {
            sb.Append($"<color={defaultColor}>{userName}: {message}</color>");
        } else { // Disable Rich Text
            sb.Append($"<color={defaultColor}>{userName}: {message}</color>");
        }

        chatText.text = sb.ToString();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) contentSizeFitter.transform);
        scrollRect.verticalNormalizedPosition = 0f;
        messageCount++;
        messageLengthQueue.Enqueue(message.Length + 1);

        if (messageCount > 16) {
            sb.Remove(0, messageLengthQueue.Peek());
            messageCount = 16;
            messageLengthQueue.Dequeue();
        }
    }

    public void OnRectUpdated() {
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void ClearChatWindow() {
        sb.Clear();
        messageCount = 0;
        messageLengthQueue.Clear();
        chatText.text = string.Empty;
    }
}
