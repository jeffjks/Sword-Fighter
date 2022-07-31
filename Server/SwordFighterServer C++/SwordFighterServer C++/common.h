//common.h
#pragma once
#include <queue>
#include <mutex>
#include <condition_variable>
#include "Packet.h"
#pragma comment(lib,"ws2_32")
#pragma warning(disable:4996)

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

static queue<MessageQueueData> messageQueue; // 메시지 처리용 큐
static mutex mtx; // 스레드 충돌 방지용