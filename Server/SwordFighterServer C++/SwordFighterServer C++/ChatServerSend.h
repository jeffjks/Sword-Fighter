/*
    패킷 전송
*/

class ChatServer;

class ChatServerSend {
private:
    unordered_map<int, Client*> *clients;

    void SendData(int clientId, Packet packet);
    void SendTCPData(int toClient, Packet packet);
    void SendTCPDataToAll(Packet packet, int fromIndex, bool exceptMe);

public:
    ChatServerSend(unordered_map<int, Client*> *_clients) {
        clients = _clients;
    }
    void WelcomeMessage(int toClient);
    void SendChatMessageAll(int fromIndex, int fromId, string msg);
};
