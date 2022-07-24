#pragma once
#include "common.h"
#include <winsock2.h>

const int dataBufferSize = 4096;

// Packet id = 1
static void GetUserId(int index, Packet packet) { // Unused
    int fromId = packet.ReadInt();
}

// Packet id = 2
static void MessageReceived(int index, Packet packet) {
    int fromId = packet.ReadInt();
    string message = packet.ReadString();
    MessageQueueData messageQueueData(index, fromId, message);
    mtx.lock();
    messageQueue.push(messageQueueData);
    mtx.unlock();
}

class ChatServer;

class Client
{
private:
    const int index;
    Packet receivedData;
    ChatServer *chatServer;
    //void(Client::*fp[1]) (int, Packet) = { &MessageReceived };
    //void(Client::*fp) (int, Packet) = &MessageReceived;
    void(*fp[2]) (int, Packet) = { GetUserId, MessageReceived }; // void 반환값, int, Packet 매개변수의 함수 포인터 선언

public:
    SOCKET clientSocket = INVALID_SOCKET;
    HANDLE evnt;
    char ip_address[50];

    Client() : index(0) {
    }

    Client(int _index, ChatServer *_chatServer) : index(_index) {
        chatServer = _chatServer;
    }

    Client(SOCKET _clientSocket, HANDLE _evnt, ChatServer *_chatServer) : index(0) {
        clientSocket = _clientSocket;
        evnt = _evnt;
        chatServer = _chatServer;
    }

    //void MessageReceived(int fromClient, Packet packet);
    void ReceiveData();
    bool HandleData(char* data, int length);
    void Disconnect();
};