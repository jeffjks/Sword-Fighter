#pragma once
#include <thread>
#include <unordered_map>
#include <WS2tcpip.h>
#include "Client.h"
#include "ChatServerSend.h"
#include "ChatServerHandle.h"

const int MAX_PLAYERS = 4;
const int PORT = 26960;
const int SERVER_MESSAGE = 127;
const int ADMIN_MESSAGE = 126;

//typedef void(ChatServerHandle::*MemFuncPtr) (int, Packet);

typedef void (ChatServerHandle::*MemFuncPtr)(int, Packet); // readability
typedef unordered_map<int, MemFuncPtr> fmap;

class ChatServer
{
private:
    SOCKET listenSocket;
    unordered_map<int, Client*> clients;

    int total_socket_count = 0;// 바인딩된 소켓과 이벤트의 종류가 지정된 이벤트 객체를 소켓 배열에 넣어준다.
    WSAEVENT handle_array[MAX_PLAYERS + 1];
    DWORD index;
    WSANETWORKEVENTS wsaNetEvents;
    ChatServerSend *chatServerSend;

public:
    fmap packetHandlers;
    ChatServerHandle *chatServerHandle;

    ChatServer() {
        chatServerSend = new ChatServerSend(&clients);
        chatServerHandle = new ChatServerHandle(&clients);
    }

    ~ChatServer() {
        delete chatServerSend;
        delete chatServerHandle;
    }

    int Start();
    void PopMessageQueue();
    void AcceptClient(int index);
    void ReceiveClientsData(int index);
    void InitializeServerData();
    void DisconnectClient(int id);
};