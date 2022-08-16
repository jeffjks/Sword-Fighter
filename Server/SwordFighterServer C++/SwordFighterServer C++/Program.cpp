#pragma once
#include "ChatServer.h"

using namespace std;

int main() {
    #ifdef _WIN32
        SetConsoleOutputCP(CP_UTF8); // 한글 깨짐 방지
    #endif

    printf("Chat Server ver %s\n", VERSION.c_str());

    ChatServer chatServer;
    chatServer.Start();
}
