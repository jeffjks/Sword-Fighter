#pragma once
#include <thread>
#include <WS2tcpip.h>
#include "Client.h"

const int MAX_PLAYERS = 4;
const int PORT = 26960;
const int SERVER_MESSAGE = 127;
const int ADMIN_MESSAGE = 126;

class ChatServer
{
private:
    SOCKET listenSocket;
    Client *clients[MAX_PLAYERS] = { NULL };

public:
    ChatServer() {

    }

    static void Broadcast();

    void InitializeServerData();
    void AcceptClient();
    int Start();
};