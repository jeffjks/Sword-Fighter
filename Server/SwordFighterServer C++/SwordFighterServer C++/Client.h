#pragma once
#include <WS2tcpip.h>
#include "Packet.h"

class ChatServerHandle; // 전방 선언

const int dataBufferSize = 4096;

class Client
{
private:
    const int index; // clients unordered_map에서의 index (key)
    Packet receivedData;
    ChatServerHandle *chatServerHandle;

public:
    SOCKET clientSocket = INVALID_SOCKET;
    char ip_address[INET_ADDRSTRLEN];
    int port;
    int id = -1; // 클라이언트의 id
    string username = "(Unknown)";

    Client(int _index, ChatServerHandle *_chatServerHandle) : index(_index) {
        chatServerHandle = _chatServerHandle;
    }

    Client(SOCKET _clientSocket, ChatServerHandle *_chatServerHandle) : index(0) {
        clientSocket = _clientSocket;
        chatServerHandle = _chatServerHandle;
    }

    void SetUserData(int _id, string _username);
    void ReceiveData();
    bool HandleData(char* data, int length);
};