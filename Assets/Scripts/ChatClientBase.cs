using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json.Linq;

public abstract class ChatClientBase : MonoBehaviour
{
    private ChatClientBase instance;

    public static int dataBufferSize = 4096;

    public string defaultIp = "127.0.0.1";
    public abstract int port { get; }
    //public int myId = 0;
    public TCP tcp;

    protected bool isConnected = false;
    protected bool isReceivedWelcomeMessage = false;
    protected delegate void ChatPacketHandler(JToken payload);
    protected Dictionary<ChatServerPackets, ChatPacketHandler> ChatPacketHandlers { get; set; }

    protected virtual void Awake() // Singleton
    {
        instance = this;
        tcp = new TCP(instance);
    }

    protected void OnApplicationQuit() {
        Disconnect();
    }

    public async Task ConnectToServer(string ip) {
        InitializeClientData();

        isConnected = true;
        await tcp.ConnectAsync(ip);
    }

    private void InitializeClientData() {
        ChatPacketHandlers = new Dictionary<ChatServerPackets, ChatPacketHandler>()
        {
            { ChatServerPackets.welcome, ChatClientHandle.Welcome },
            { ChatServerPackets.chatMessage, ChatClientHandle.GetChatMessage },
            { ChatServerPackets.joinChatRoom, ChatClientHandle.JoinChatRoom },
            { ChatServerPackets.leaveChatRoom, ChatClientHandle.LeaveChatRoom },
        };
        Debug.Log("Initialize chat packets.");
    }

    public class TCP
    {
        public TcpClient socket;

        private readonly ChatClientBase instance;
        private NetworkStream stream;
        private StringBuilder jsonBuffer = new StringBuilder();
        private byte[] receiveBuffer;

        public TCP(ChatClientBase instance) {
            this.instance = instance;
        }

        public async UniTask ConnectAsync(string ip)
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            
            var connectTask = socket.ConnectAsync(System.Net.IPAddress.Parse(ip), instance.port);

            var timeoutTask = Task.Delay(5000);
            var completedTask = await Task.WhenAny(connectTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                socket.Close();
                throw new TimeoutException("TCP connect timeout");
            }

            if (!socket.Connected)
            {
                socket.Close();
                throw new Exception("Failed to connect to chat server");
            }

            stream = socket.GetStream();

            _ = ReceiveLoopAsync(); // 비동기 수신 시작
        }

        private async UniTask ReceiveLoopAsync()
        {
            try
            {
                while (true)
                {
                    int byteLength = await stream.ReadAsync(receiveBuffer, 0, dataBufferSize);

                    if (byteLength <= 0)
                    {
                        Disconnect();
                        break;
                    }

                    string receivedText = Encoding.UTF8.GetString(receiveBuffer, 0, byteLength);
                    HandleJsonData(receivedText);
                }
            }
            catch
            {
                Disconnect();
            }
        }

        public async UniTask SendDataAsync(byte[] data)
        {
            try
            {
                if (socket != null && stream != null)
                {
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error sending data to chat server via TCP: {e}");
            }
        }
        
        private void HandleJsonData(string data) {
            jsonBuffer.Append(data);

            string bufferContent = jsonBuffer.ToString();
            int newlineIndex;

            while ((newlineIndex = bufferContent.IndexOf('\n')) >= 0)
            {
                string line = bufferContent.Substring(0, newlineIndex).Trim();
                bufferContent = bufferContent.Substring(newlineIndex + 1);

                if (!string.IsNullOrWhiteSpace(line))
                {
                    try
                    {
                        var root = JObject.Parse(line);
                        int type = Convert.ToInt32(root["Type"]);
                        
                        ThreadManager.ExecuteOnMainThread(() =>
                        {
                            var chatServerPacketType = (ChatServerPackets) type;
                            var jToken = root["Payload"];
                            if (instance.IsConnected()) { // 접속 종료 시 패킷 처리 중지
                                instance.ChatPacketHandlers[chatServerPacketType](jToken);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"JSON parse error: {ex.Message}");
                    }
                }
            }

            jsonBuffer.Clear();
            jsonBuffer.Append(bufferContent); // 남은 조각 다시 버퍼에 저장
        }

        private void Disconnect() {
            instance.Disconnect();

            stream = null;
            jsonBuffer = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    virtual public void Disconnect() {
        isConnected = false;
        tcp.socket.Close();
    }

    public bool IsConnected() {
        return isConnected;
    }

    public bool IsReceivedWelcomeMessage() {
        return isReceivedWelcomeMessage;
    }
}
