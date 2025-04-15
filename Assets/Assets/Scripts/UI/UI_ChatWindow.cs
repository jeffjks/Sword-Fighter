using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Text;
using TMPro;

struct MessageInfo {
    public string messageType;
    public string colorCode;

    public MessageInfo(string messageType, string colorCode) {
        this.messageType = messageType;
        this.colorCode = colorCode;
    }
};

enum MessageType {
    ERROR_MESSAGE = -1,
    SYSTEM_MESSAGE = 126,
    SERVER_MESSAGE = 127,
};

public class UI_ChatWindow : MonoBehaviour
{
    public TextMeshProUGUI chatText;
    public ScrollRect scrollRect;
    public ContentSizeFitter contentSizeFitter;

    private StringBuilder sb = new StringBuilder();
    private int messageCount = 0;
    private Queue<int> messageLengthQueue = new Queue<int>();
    private Dictionary<int, MessageInfo> specialMessage = new Dictionary<int, MessageInfo>();

    UI_ChatWindow() {
        specialMessage[(int) MessageType.ERROR_MESSAGE] = new MessageInfo("[오류]", "#FF0000FF"); // Red
        specialMessage[(int) MessageType.SYSTEM_MESSAGE] = new MessageInfo("[시스템]", "#DF7401FF"); // Dark Orange
        specialMessage[(int) MessageType.SERVER_MESSAGE] = new MessageInfo("[공지]", "#DF7401FF"); // Dark Orange
    }

    public void PushTextMessage(int fromId, string message) {
        string userName, defaultColor;
        if (specialMessage.ContainsKey(fromId)) {
            userName = specialMessage[fromId].messageType;
            defaultColor = specialMessage[fromId].colorCode;
        }
        else {
            userName = GameManager.instance.GetUserNameWithId(fromId);
            defaultColor = "#000000FF"; // Black
        }

        if (messageCount > 0) {
            sb.Append("\n");
        }

        sb.Append($"<color={defaultColor}>{userName}: {message}</color>");
        
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
