#pragma once
#include "ChatServerHandle.h"
#include "ChatServerSend.h"

// Packet id = 1 : 클라이언트 id 획득
void ChatServerHandle::WelcomeMessageReceived(int index, Packet packet) {
    int id = packet.ReadInt();
    string username = packet.ReadString();
    (*clients)[index]->SetUserData(id, username);

    char* ip_address = (*clients)[index]->ip_address;
    int port = (*clients)[index]->port;

    printf("%s (%s:%d) connected successfully and is now player %d.\n", username.c_str(), ip_address, port, id);
    chatServerSend->SendClientStateNotice(index, id, ClientState::CLIENT_JOINED);
}

// Packet id = 2 : 클라이언트 채팅 메세지
void ChatServerHandle::MessageReceived(int index, Packet packet) {
    int fromId = packet.ReadInt();
    string message = packet.ReadString();

    chatServerSend->PushMessageQueueData(index, fromId, message);
}

void ChatServerHandle::HandlePacketId(int packetId, int index, Packet packet) {
    (this->*packetHandlers[packetId])(index, packet);
}

void ChatServerHandle::InitializePacketHandlers() {
    packetHandlers[ChatClientPackets::welcomeMessageReceived] = &ChatServerHandle::WelcomeMessageReceived;
    packetHandlers[ChatClientPackets::chatClientMessage] = &ChatServerHandle::MessageReceived;
}


