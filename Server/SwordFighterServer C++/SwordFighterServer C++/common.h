//common.h
#pragma once
#include <queue>
#include <mutex>
#include <condition_variable>
#include <iostream>
#include <winsock2.h>
#include "Packet.h"
#pragma comment(lib,"ws2_32")
#pragma warning(disable:4996)

struct MessageQueueData {
    int clientIndex;
    int fromId;
    string message;

    MessageQueueData(int _clientIndex, int _fromId, string _message) {
        clientIndex = _clientIndex;
        fromId = _fromId;
        message = _message;
    }
};

static unordered_map<int, Client*> clients;
static queue<MessageQueueData> messageQueue;
static mutex mtx;