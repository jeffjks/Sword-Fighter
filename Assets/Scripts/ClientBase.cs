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
    protected Dictionary<int, PacketHandler> PacketHandlers { get; set; }

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

    private void InitializeClientData() {
        PacketHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int) ServerPackets.welcome, ClientHandle.Welcome },
            { (int) ServerPackets.requestServerTime, ClientHandle.RequestServerTime },
            { (int) ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
            { (int) ServerPackets.updatePlayerPosition, ClientHandle.UpdatePlayerPosition },
            { (int) ServerPackets.playerSkill, ClientHandle.PlayerSkill },
            { (int) ServerPackets.playerState, ClientHandle.PlayerState },
            { (int) ServerPackets.playerHp, ClientHandle.PlayerHp },
            { (int) ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnected },
        };
        Debug.Log("Initialize packets.");
    }

    public class TCP
    {
        public TcpClient socket;

        private readonly ClientBase instance;
        private NetworkStream _stream;
        private Packet _receivedData;
        private byte[] _receiveBuffer;

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

            _receiveBuffer = new byte[dataBufferSize];
            
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

            _stream = socket.GetStream();
            _receivedData = new Packet();

            _ = ReceiveLoopAsync(); // 비동기 수신 시작
        }

        private async UniTask ReceiveLoopAsync()
        {
            try
            {
                while (true)
                {
                    int byteLength = await _stream.ReadAsync(_receiveBuffer, 0, dataBufferSize).ConfigureAwait(false);

                    if (byteLength <= 0)
                    {
                        Disconnect();
                        break;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(_receiveBuffer, data, byteLength);

                    _receivedData.Reset(HandleData(data)); // 기존 동작 유지
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
                if (socket != null && _stream != null)
                {
                    await _stream.WriteAsync(packet.ToArray(), 0, packet.Length()).ConfigureAwait(false);
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

            _receivedData.SetBytes(data);

            if (_receivedData.UnreadLength() >= 4) {
                packetLength = _receivedData.ReadInt(); // 패킷 길이 (패킷 가장 첫 부분)
                if (packetLength <= 0) {
                    return true;
                }
            }

            while (0 < packetLength && packetLength <= _receivedData.UnreadLength()) {
                byte[] packetBytes = _receivedData.ReadBytes(packetLength);

                if (GameManager.IsDebugPing)
                {
                    HandlePacketWithDelay(packetBytes).Forget();
                }
                else
                {
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        var packet = new Packet(packetBytes);
                        int packetId = packet.ReadInt(); // 패킷 종류 (SpawnPlayer, PlayerMovement, ChatMessage 등)
                        if (instance.IsConnected()) { // 접속 종료 시 패킷 처리 중지
                            instance.PacketHandlers[packetId](packet);
                        }
                    });
                }

                packetLength = 0;

                if (_receivedData.UnreadLength() >= 4) { // 아직 패킷 길이가 남아있음 = 동시에 여러 종류의 패킷이 들어왔을 경우
                    packetLength = _receivedData.ReadInt(); // 읽은 Integer를 패킷 길이로 취급하여 패킷 읽기 계속 진행
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

            ThreadManager.ExecuteOnMainThread(() =>
            {
                var packet = new Packet(packetBytes);
                int packetId = packet.ReadInt(); // 패킷 종류 (SpawnPlayer, PlayerMovement, ChatMessage 등)
                if (instance.IsConnected()) { // 접속 종료 시 패킷 처리 중지
                    instance.PacketHandlers[packetId](packet);
                }
            });
        }

        public void Disconnect() {
            _stream?.Close();
            _stream = null;
            socket?.Close();
            socket = null;

            _receivedData = null;
            _receiveBuffer = null;

            instance.isConnected = false;
        }
    }

    public virtual void Disconnect() {
        if (isConnected == false)
            return;
        isConnected = false;
        tcp?.Disconnect();
    }

    public bool IsConnected() {
        return isConnected;
    }
}
