#pragma once
#include "ChatServerSend.h"

// index : 채팅 주인 client의 index
// fromId : 채팅 주인 유저 id

// 데이터 최종 전송
void ChatServerSend::SendData(int toIndex, Packet packet) {
    const char* send_buffer = packet.ToArray();
    send((*clients)[toIndex]->clientSocket, send_buffer, packet.Length(), 0);
}

// 맨 앞에 패킷 총 길이 붙이기
void ChatServerSend::SendTCPData(int toIndex, Packet packet) {
    packet.WriteLength();
    SendData(toIndex, packet);
}

// 다른 유저 전체에게 패킷 전송
void ChatServerSend::SendTCPDataToAll(Packet packet, int fromIndex, bool exceptMe = false) {
    packet.WriteLength();

    for (int i = 1; i <= MAX_PLAYERS; ++i) {
        if ((*clients)[i]->clientSocket == INVALID_SOCKET) {
            continue;
        }
        if (fromIndex == i && exceptMe) { // exceptMe가 true면 자기 자신은 제외
            continue;
        }
        //cout << i << ", " << fromId << ", " << exceptMe << endl;
        SendData(i, packet);
    }
}


// Packets

// 최초 접속 시 메세지 전송 + 클라이언트 id 요청
void ChatServerSend::WelcomeMessage(int toIndex) {
    Packet packet((int)ChatServerPackets::welcomeMessage); // packet id

    string msg = string(u8"채팅 서버에 접속하셨습니다.");

    packet.Write(MessageType::SERVER_MESSAGE);
    packet.Write(msg);

    SendTCPData(toIndex, packet);
}

// 플레이어 상태 알림 메세지
void ChatServerSend::SendClientStateNotice(int fromIndex, int fromId, int state) {
    Packet packet((int)ChatServerPackets::clientStateNotice); // packet id
    string username = (*clients)[fromIndex]->username;

    packet.Write(fromId); // 대상 클라이언트 id
    packet.Write(username);
    packet.Write(state); // 대상 클라이언트 id의 상태

    SendTCPDataToAll(packet, fromIndex, true);
}

// 채팅 전송
void ChatServerSend::SendChatMessageAll(int fromIndex, int fromId, string msg) {
    Packet packet((int)ChatServerPackets::chatServerMessage); // packet id

    packet.Write(fromId); // 채팅 주인
    packet.Write(msg); // 채팅 내용

    SendTCPDataToAll(packet, fromIndex, true);
}



void ChatServerSend::PushMessageQueueData(int index, int fromId, string message) { // Producer
    MessageQueueData messageQueueData = MessageQueueData(index, fromId, message);

    // Critical Section
    mtx_messageQueue.lock();
    messageQueue.push(messageQueueData);
    mtx_messageQueue.unlock();

    ctrl_var.notify_one(); // Consumer에게 알림
}

void ChatServerSend::PopMessageQueueData() { // Consumer
    while (true) {
        unique_lock<mutex> lock(mtx_messageQueue);

        ctrl_var.wait(lock, [&]() { return !messageQueue.empty(); }); // Block 상태로 돌입. Block 해제 시 mutex lock 후 진행
        // queue에 메세지가 존재하면 wait 하지 않고 mutex lock 후 진행

        while (!messageQueue.empty()) { // 큐에 쌓인 모든 메세지 처리
            MessageQueueData messageQueueData = messageQueue.front();
            SendChatMessageAll(messageQueueData.clientIndex, messageQueueData.fromId, messageQueueData.message);
            messageQueue.pop();
        }
        lock.unlock();
        this_thread::sleep_for(chrono::milliseconds(MS_BROADCASTING)); // 최소 대기 시간
    }
}