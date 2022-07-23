using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Text;

public class ChatMessageWindow : MonoBehaviour
{
    const int SERVER_MESSAGE = 127;
    const int ADMIN_MESSAGE = 126;
    
    public Text chatText;
    public ScrollRect scrollRect;

    private StringBuilder sb = new StringBuilder();
    private int messageCount = 0;
    private Queue<int> messageLengthQueue = new Queue<int>();

    public void PushTextMessage(int fromId, string message) {
        string userName;
        if (fromId == SERVER_MESSAGE) {
            userName = "[공지]";
        }
        else if (fromId == ADMIN_MESSAGE) {
            userName = "[관리자]";
        }
        else {
            try {
                userName = GameManager.players[fromId].username;
            }
            catch (KeyNotFoundException) {
                userName = "(Unknown)";
            }
        }

        if (messageCount > 0) {
            sb.Append("\n");
        }
        sb.Append($"{userName}: {message}");
        chatText.text = sb.ToString();
        scrollRect.verticalNormalizedPosition = 0f;
        messageCount++;
        messageLengthQueue.Enqueue(message.Length + 1);

        if (messageCount > 16) {
            sb.Remove(0, messageLengthQueue.Peek());
            messageCount = 16;
            messageLengthQueue.Dequeue();
        }
    }

    public void ClearChatMessageWindow() {
        sb.Clear();
        messageCount = 0;
        messageLengthQueue.Clear();
    }
}
