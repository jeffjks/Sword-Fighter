#pragma once
#include "common.h"

class Client;

class ChatServerHandle {
public:
    unordered_map<int, Client*> *clients;

    void GetUserId(int index, Packet packet);
    void MessageReceived(int index, Packet packet);
};