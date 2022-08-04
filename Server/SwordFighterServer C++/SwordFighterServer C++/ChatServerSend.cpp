#pragma once
#include "ChatServer.h"

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

// 최초 접속 시 메시지 전송 + 클라이언트 id 요청
void ChatServerSend::WelcomeMessage(int toIndex) {
    Packet packet = Packet(ChatServerPackets::welcomeMessage);

    string msg = string(u8"채팅 서버에 접속하셨습니다.");

    packet.Write(SERVER_MESSAGE);
    packet.Write(msg);

    SendTCPData(toIndex, packet);
}

// 채팅 전송
void ChatServerSend::SendChatMessageAll(int fromIndex, int fromId, string msg) {
    Packet packet((int)ChatServerPackets::chatServerMessage); // packet id

    packet.Write(fromId); // 채팅 주인
    packet.Write(msg); // 채팅 내용

    SendTCPDataToAll(packet, fromIndex, true);
}