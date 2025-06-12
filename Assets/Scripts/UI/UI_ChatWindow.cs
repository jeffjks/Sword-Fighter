using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Text;
using TMPro;

struct MessageInfo {
    public string headMessage;
    public string colorCode;

    public MessageInfo(string headMessage, string colorCode) {
        this.headMessage = headMessage;
        this.colorCode = colorCode;
    }
};

public enum MessageType {
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
    private readonly Dictionary<MessageType, MessageInfo> _specialMessage = new() {
        {MessageType.ERROR_MESSAGE, new MessageInfo("[오류]", "#FF0000FF")}, // Red
        {MessageType.SYSTEM_MESSAGE, new MessageInfo("[시스템]", "#DF7401FF")}, // Dark Orange
        {MessageType.SERVER_MESSAGE, new MessageInfo("[공지]", "#DF7401FF")} // Dark Orange
    };

    private const string DefaultColor = "#000000FF"; // Black
    
    public void PushSpecialMessage(MessageType messageType, string message)
    {
        if (_specialMessage.TryGetValue(messageType, out var messageInfo))
        {
            DisplayTextMessage(messageInfo.colorCode, messageInfo.headMessage, message);
        }
        else
        {
            Debug.LogError($"Uknown Message Type: {messageType}.\nMessage: {message}");
        }
    }

    public void PushTextMessage(int fromId, string message)
    {
        var userName = GameManager.instance.GetUserNameWithId(fromId);
        DisplayTextMessage(DefaultColor, $"{userName}:", message);
    }

    private void DisplayTextMessage(string colorCode, string head, string message)
    {
        if (messageCount > 0) {
            sb.Append("\n");
        }

        sb.Append($"<color={colorCode}>{head} {message}</color>");
        
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
