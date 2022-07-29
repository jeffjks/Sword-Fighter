#pragma once
#include "ChatServer.h"

// Packet id = 1
void ChatServerHandle::WelcomeMessageReceived(int index, Packet packet) {
    int id = packet.ReadInt();
    (*clients)[index]->id = id;

    printf("%s:%d connected successfully and is now player %d.\n", (*clients)[index]->ip_address, (*clients)[index]->port, (*clients)[index]->id);
}

// Packet id = 2
void ChatServerHandle::MessageReceived(int index, Packet packet) {
    int fromId = packet.ReadInt();
    string message = packet.ReadString();
    MessageQueueData messageQueueData = MessageQueueData(index, fromId, message);
    mtx.lock();
    messageQueue.push(messageQueueData);
    mtx.unlock();
}