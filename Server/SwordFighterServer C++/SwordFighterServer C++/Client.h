#pragma once
#include "common.h"
#include <winsock2.h>

const int dataBufferSize = 4096;

// Packet id = 1
static void MessageReceived(int fromClient, Packet packet) {
    string str = packet.ReadString();
    mtx.lock();
    messageQueue.push(make_pair(fromClient, str));
    mtx.unlock();
}

class ChatServer;

class Client
{
private:
    const int id;
    Packet receivedData;
    ChatServer *chatServer;
    //void(Client::*fp[1]) (int, Packet) = { &MessageReceived };
    //void(Client::*fp) (int, Packet) = &MessageReceived;
    void(*fp[1]) (int, Packet) = { MessageReceived }; // void 반환값, int, Packet 매개변수의 함수 포인터 선언

public:
    SOCKET clientSocket = INVALID_SOCKET;
    HANDLE evnt;
    char ip_address[50];

    Client() : id(0) {
    }

    Client(int _id, ChatServer *_chatServer) : id(_id) {
        chatServer = _chatServer;
    }

    Client(SOCKET _clientSocket, HANDLE _evnt, ChatServer *_chatServer) : id(0) {
        clientSocket = _clientSocket;
        evnt = _evnt;
        chatServer = _chatServer;
    }

    //void MessageReceived(int fromClient, Packet packet);
    void ReceiveData();
    bool HandleData(char* data, int length);
    void Disconnect();
};