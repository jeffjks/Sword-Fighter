#pragma once
#include <thread>
#include <unordered_map>
#include <WS2tcpip.h>
#include <unordered_map>
#include "common.h"
#include "Client.h"
#include "ChatServerSend.h"

const int MAX_PLAYERS = 4;
const int PORT = 26960;
const int SERVER_MESSAGE = 127;
const int ADMIN_MESSAGE = 126;

class ChatServer
{
private:
    SOCKET listenSocket;
    //Client *clients[MAX_PLAYERS + 1] = { NULL };

    int total_socket_count = 0;// 바인딩된 소켓과 이벤트의 종류가 지정된 이벤트 객체를 소켓 배열에 넣어준다.
    WSAEVENT handle_array[MAX_PLAYERS + 1];
    //SOCKET hSocketArray[WSA_MAXIMUM_WAIT_EVENTS] = {};
    //WSAEVENT hSocketEventArray[MAX_PLAYERS + 1] = {};
    DWORD index;
    WSANETWORKEVENTS wsaNetEvents;
    ChatServerSend chatServerSend;

public:
    ChatServer() {
        chatServerSend = ChatServerSend();
    }

    int Start();
    void PopMessageQueue();
    void InitializeServerData();
    void AcceptClient(int index);
    void ReceiveClientsData(int index);
    void DisconnectClient(int id);
};