#pragma once
#include <queue>
#include <mutex>
#include <condition_variable>
#include "ChatServerHandle.h"
#include "common.h"

/*
    패킷 전송
*/

//class Client;

struct MessageQueueData { // 채팅 메시지 구성
    int clientIndex; // 보낸 클라이언트의 index
    int fromId; // 보낸 클라이언트의 id
    string message; // 메세지 내용

    MessageQueueData(int _clientIndex, int _fromId, string _message) {
        clientIndex = _clientIndex;
        fromId = _fromId;
        message = _message;
    }
};

class ChatServerSend {
private:
    unordered_map<int, Client*> *clients;

    void SendData(int clientId, Packet packet);
    void SendTCPData(int toClient, Packet packet);
    void SendTCPDataToAll(Packet packet, int fromIndex, bool exceptMe);
    mutex mtx; // 스레드 충돌 방지용

public:
    queue<MessageQueueData> messageQueue; // 메시지 처리용 큐

    ChatServerSend(unordered_map<int, Client*> *_clients) {
        clients = _clients;
    }
    void WelcomeMessage(int toClient);
    void SendChatMessageAll(int fromIndex, int fromId, string msg);

    void PushMessageQueueData(int index, int fromId, string message);
    void PopMessageQueueData();
};