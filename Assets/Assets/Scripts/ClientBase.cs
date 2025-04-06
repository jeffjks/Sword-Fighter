using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;

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

    public void ConnectToServer(string ip) {
        InitializeClientData();

        isConnected = true;
        tcp.Connect(ip);
    }

    public class TCP
    {
        public TcpClient socket;

        private ClientBase instance;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public TCP(ClientBase instance) {
            this.instance = instance;
        }

        public void Connect(string ip) {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            IAsyncResult result = socket.BeginConnect(System.Net.IPAddress.Parse(ip), instance.port, ConnectCallback, socket);

            result.AsyncWaitHandle.WaitOne(5000, true);

            if (!socket.Connected) {
                socket.Close();
                throw new TimeoutException();
            }
        }

        private void ConnectCallback(IAsyncResult result) {
            socket.EndConnect(result);

            if (!socket.Connected) {
                return;
            }

            stream = socket.GetStream();
            receivedData = new Packet();
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet packet) {
            try {
                if (socket != null) {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception e) {
                Debug.Log($"Error sending data to server via TCP: {e}");
            }
        }

        private void ReceiveCallback(IAsyncResult result) {
            try {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0) {
                    Disconnect();
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                receivedData.Reset(HandleData(data)); 
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch {
                Disconnect();
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
                        using (Packet packet = new Packet(packetBytes)) {
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

            if (packetLength <= 1) {
                return true;
            }
            return false;
        }
        
        private async UniTaskVoid HandlePacketWithDelay(byte[] packetBytes)
        {
            await UniTask.SwitchToMainThread(); // 메인 스레드로 전환

            int ping = UnityEngine.Random.Range(GameManager.instance.m_PingMin, GameManager.instance.m_PingMax) / 2;

            if (ping > 0)
                await UniTask.Delay(ping);

            using (Packet packet = new Packet(packetBytes)) {
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
