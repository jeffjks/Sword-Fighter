#pragma once
#include "ChatServer.h"

// index : 채팅 주인 client의 index
// fromId : 채팅 주인 유저 id

void ChatServerSend::SendData(int toIndex, Packet packet) {
    const char* send_buffer = packet.ToArray();
    send((*clients)[toIndex]->clientSocket, send_buffer, packet.Length(), 0);
}


void ChatServerSend::SendTCPData(int toIndex, Packet packet) {
    packet.WriteLength();
    SendData(toIndex, packet);
}

void ChatServerSend::SendTCPDataToAll(Packet packet, int fromIndex, bool exceptMe = false) {
    packet.WriteLength();

    for (int i = 1; i <= MAX_PLAYERS; ++i) {
        if ((*clients)[i]->clientSocket == INVALID_SOCKET) {
            continue;
        }
        if (fromIndex == i && exceptMe) {
            continue;
        }
        //cout << i << ", " << fromId << ", " << exceptMe << endl;
        SendData(i, packet);
    }
}


// Packets
void ChatServerSend::WelcomeMessage(int toIndex) {
    Packet packet = Packet(ChatServerPackets::chatServerMessage);

    string msg = string(u8"환영합니다.");

    packet.Write(SERVER_MESSAGE);
    packet.Write(msg);

    SendTCPData(toIndex, packet);
}

void ChatServerSend::SendChatMessageAll(int fromIndex, int fromId, string msg) {

    Packet packet((int)ChatServerPackets::chatServerMessage); // packet id

    packet.Write(fromId); // 채팅 주인
    packet.Write(msg); // 채팅 내용

    SendTCPDataToAll(packet, fromIndex, true);
}