#pragma once
#include "common.h"
#include <winsock2.h>

const int dataBufferSize = 4096;

static void MessageReceived(int fromClient, Packet packet) {
    string str = packet.ReadString();
    //mtx.lock();
    messageQueue.push(make_pair(fromClient, str));
    //mtx.unlock();
}

class Client
{
private:
    const int id;
    Packet receivedData;
    //void(Client::*fp[1]) (int, Packet) = { &MessageReceived };
    //void(Client::*fp) (int, Packet) = &MessageReceived;
    void(*fp[1]) (int, Packet) = { MessageReceived }; // void 반환값, int, Packet 매개변수의 함수 포인터 선언

public:
    SOCKET clientSocket = INVALID_SOCKET;

    Client() : id(0) {
    }

    Client(int _id) : id(_id) {
    }

    //void MessageReceived(int fromClient, Packet packet);
    void Connect();
    bool HandleData(char* data, int length);
};