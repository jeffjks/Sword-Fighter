using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public abstract class ClientBase : MonoBehaviour
{
    private ClientBase instance;
    public static int dataBufferSize = 4096;

    public string defaultIp = "127.0.0.1";
    public abstract int port { get; }
    //public int myId = 0;
    public TCP tcp;

    protected bool isConnected = false;
    protected delegate void PacketHandler(Packet packet);
    protected abstract Dictionary<int, PacketHandler> packetHandlers { get; set; }

    protected abstract void InitializeClientData();

    protected virtual void Awake() // Singleton
    {
        instance = this;
    }

    protected void Start()
    {
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

    public class TCP
    {
        public TcpClient socket;

        private readonly ClientBase instance;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public TCP(ClientBase instance) {
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
                throw new Exception("Failed to connect to server");
            }

            stream = socket.GetStream();
            receivedData = new Packet();

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

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivedData.Reset(HandleData(data)); // 기존 동작 유지
                }
            }
            catch
            {
                Disconnect();
            }
        }

        public async UniTask SendDataAsync(Packet packet)
        {
            try
            {
                if (socket != null && stream != null)
                {
                    await stream.WriteAsync(packet.ToArray(), 0, packet.Length());
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error sending data to server via TCP: {e}");
            }
        }
        
        // TCP는 데이터의 순서를 보장해주지만 데이터의 크기까지는 보장하지 않는다. 받은 데이터가 데이터 전체인지 일부분만인지 체크 필요
        private bool HandleData(byte[] data) {
            int packetLength = 0;

            receivedData.SetBytes(data);

            if (receivedData.UnreadLength() >= 4) {
                packetLength = receivedData.ReadInt(); // 패킷 길이 (패킷 가장 첫 부분)
                if (packetLength <= 0) {
                    return true;
                }
            }

            while (0 < packetLength && packetLength <= receivedData.UnreadLength()) {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);

                if (GameManager.IsDebugPing)
                {
                    HandlePacketWithDelay(packetBytes).Forget();
                }
                else
                {
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new (packetBytes)) {
                            int packetId = packet.ReadInt(); // 패킷 종류 (SpawnPlayer, PlayerMovement, ChatMessage 등)
                            if (instance.IsConnected()) { // 접속 종료 시 패킷 처리 중지
                                instance.packetHandlers[packetId](packet);
                            }
                        }
                    });
                }

                packetLength = 0;

                if (receivedData.UnreadLength() >= 4) { // 아직 패킷 길이가 남아있음 = 동시에 여러 종류의 패킷이 들어왔을 경우
                    packetLength = receivedData.ReadInt(); // 읽은 Integer를 패킷 길이로 취급하여 패킷 읽기 계속 진행
                    if (packetLength <= 0) {
                        return true;
                    }
                }
            }

            if (packetLength <= 0) {
                return true;
            }
            return false;
        }
        
        private async UniTaskVoid HandlePacketWithDelay(byte[] packetBytes)
        {
            await UniTask.SwitchToMainThread(); // 메인 스레드로 전환

            int ping = GameManager.instance.GetDebugPing() / 2;

            if (ping > 0)
                await UniTask.Delay(ping);

            using (Packet packet = new (packetBytes)) {
                int packetId = packet.ReadInt();
                if (instance.IsConnected()) {
                    instance.packetHandlers[packetId](packet);
                }
            }
        }

        private void Disconnect() {
            instance.Disconnect();

            stream = null;
            receivedData = null;
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
}
