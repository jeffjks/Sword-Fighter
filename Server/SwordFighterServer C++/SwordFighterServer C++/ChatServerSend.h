#pragma once
#include "common.h"
#include <winsock2.h>

class ChatServerSend {
private:
    void SendData(int clientId, Packet packet);
    void SendTCPData(int toClient, Packet packet);
    void SendTCPDataToAll(Packet packet, int fromIndex, bool exceptMe = false);

public:
    void WelcomeMessage(int toClient);
    void SendChatMessageAll(int fromIndex, int fromId, string msg);
};
