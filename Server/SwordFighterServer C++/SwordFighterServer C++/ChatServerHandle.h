#pragma once
#include <unordered_map>
#include "Client.h"

/*
    받은 패킷 처리
*/

class ChatServerSend; // 전방 선언

typedef void (ChatServerHandle::*MemFuncPtr)(int, Packet); // 함수 포인터
typedef unordered_map<int, MemFuncPtr> fmap;

class ChatServerHandle {
private:
    ChatServerSend *chatServerSend;

    void WelcomeMessageReceived(int index, Packet packet);
    void MessageReceived(int index, Packet packet);

public:
    unordered_map<int, Client*> *clients;
    fmap packetHandlers; // 함수 포인터를 활용한 packetId 작업

    ChatServerHandle(unordered_map<int, Client*> *_clients, ChatServerSend *_chatServerSend) {
        clients = _clients;
        chatServerSend = _chatServerSend;
    }

    void HandlePacketId(int packetId, int index, Packet packet);
    void InitializePacketHandlers();
};