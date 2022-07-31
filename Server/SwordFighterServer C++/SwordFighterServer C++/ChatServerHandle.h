#pragma once
#include "common.h"

/*
    받은 패킷 처리
*/

class Client;

class ChatServerHandle {
public:
    unordered_map<int, Client*> *clients;

    ChatServerHandle(unordered_map<int, Client*> *_clients) {
        clients = _clients;
    }

    void WelcomeMessageReceived(int index, Packet packet);
    void MessageReceived(int index, Packet packet);
};