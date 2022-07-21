using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Text;

public class ChatMessageWindow : MonoBehaviour
{
    public Text chatText;
    public ScrollRect scrollRect;

    private StringBuilder sb = new StringBuilder();
    private int messageCount = 0;
    private Queue<int> messageLengthQueue = new Queue<int>();

    public void PushTextMessage(string message) {
        if (messageCount > 0) {
            sb.Append("\n");
        }
        sb.Append(message);
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
}
