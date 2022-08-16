#pragma once
#include "ChatServer.h"

using namespace std;

/*
    Server
*/

// 스레드를 사용한 메세지 큐에 담긴 메세지 Broadcast
void ChatServer::PopMessageQueue() {
    chatServerSend->PopMessageQueueData();
}

// listen 소켓으로부터 클라이언트 Accept
void ChatServer::AcceptClient(int index) {
    int currentIndex = -1;
    if (wsaNetEvents.iErrorCode[FD_ACCEPT_BIT] != 0) {
        cout << "Accept Error!" << endl;
        return;
    }
    if (total_socket_count == FD_SETSIZE) {
        cout << "Total socket counts are max!" << endl;
        return;
    }

    SOCKADDR_IN acceptClientSockaddr;
    int clientSize = sizeof(SOCKADDR_IN);

    SOCKET acceptClientSocket = accept(listenSocket, (SOCKADDR*)& acceptClientSockaddr, &clientSize); // accept한 클라이언트는 clientSocket으로 통신

    if (acceptClientSocket == INVALID_SOCKET)
    {
        cerr << "Can't accept a socket!" << endl;
        closesocket(listenSocket);
        WSACleanup();
        return;
    }

    for (int i = 1; i <= MAX_PLAYERS; i++) // 비어있는 가장 첫 clients Dictionary에 배정
    {
        if (clients[i]->clientSocket == INVALID_SOCKET)
        {
            clients[i]->clientSocket = acceptClientSocket;

            handle_array[i] = WSACreateEvent();

            WSAEventSelect(acceptClientSocket, handle_array[i], FD_READ | FD_CLOSE);
            total_socket_count++;
            currentIndex = i;
            break;
        }
    }

    int clientPort = acceptClientSockaddr.sin_port; // port 저장
    char clientAddress[INET_ADDRSTRLEN];
    inet_ntop(AF_INET, &(acceptClientSockaddr.sin_addr), clientAddress, INET_ADDRSTRLEN); // ip 저장

    if (currentIndex == -1) {
        closesocket(acceptClientSocket);
        printf("%s:%d failed to connect: Server full!\n", clientAddress, clientPort);
        return;
    }

    strcpy_s(clients[currentIndex]->ip_address, clientAddress);
    clients[currentIndex]->port = clientPort;

    printf("Incoming connection from %s:%d\n", clientAddress, clientPort);

    chatServerSend->WelcomeMessage(currentIndex); // Send Welcome Message
}

// RECV
void ChatServer::ReceiveClientsData(int index) {
    if (wsaNetEvents.iErrorCode[FD_READ_BIT] != 0)
    {
        cout << "Recv Error!" << endl;
        return;
    }
    clients[index]->ReceiveData();
    return;
}

int ChatServer::Start() {
    WSADATA wsaData;
    int iniResult = WSAStartup(MAKEWORD(2, 2), &wsaData); // Winsock2 초기화
    if (iniResult != 0)
    {
        cerr << "Can't Initialize winsock!" << endl;
        return -1;
    }

    listenSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP); // 리스닝 모드 소켓
    if (listenSocket == INVALID_SOCKET)
    {
        cerr << "Can't create a socket!" << endl;
        WSACleanup();
        return -2;
    }

    SOCKADDR_IN servAddr;
    servAddr.sin_family = AF_INET;
    servAddr.sin_addr.S_un.S_addr = htonl(INADDR_ANY);
    servAddr.sin_port = htons(PORT); // int 자료형을 네트워크 byte order로 변경 (Big Endian)

    int bindResult = ::bind(listenSocket, reinterpret_cast<sockaddr*>(&servAddr), sizeof(servAddr));
    if (bindResult == SOCKET_ERROR)
    {
        cerr << "Can't bind a socket! Quitting" << endl;
        closesocket(listenSocket);
        WSACleanup();
        return -3;
    }

    cout << "Starting Server..." << endl;
    int listenResult = listen(listenSocket, SOMAXCONN);
    if (listenResult == SOCKET_ERROR)
    {
        cerr << "Can't listen a socket! Quitting" << endl;
        closesocket(listenSocket);
        WSACleanup();
        return -4;
    }

    //WSAEVENT wsaEvent = WSACreateEvent();
    handle_array[0] = WSACreateEvent();
    clients[0] = new Client(listenSocket, chatServerHandle); // 리스닝 전용 소켓은 clients 0번에 배정

    WSAEventSelect(listenSocket, handle_array[0], FD_ACCEPT);
    total_socket_count++;

    InitializeServerData();

    thread(&ChatServer::PopMessageQueue, this).detach();

    printf("Chat Server started on %d.\n", PORT);

    while (true) { // WSA Event로 비동기 구현
        wsaIndex = WSAWaitForMultipleEvents(total_socket_count, handle_array, false, 5000, false);

        if ((wsaIndex != WSA_WAIT_FAILED) && (wsaIndex != WSA_WAIT_TIMEOUT))
        {
            WSAEnumNetworkEvents(clients[wsaIndex]->clientSocket, handle_array[wsaIndex], &wsaNetEvents);
            if (wsaNetEvents.lNetworkEvents == FD_ACCEPT)
                AcceptClient(wsaIndex);
            else if (wsaNetEvents.lNetworkEvents == FD_READ)
                ReceiveClientsData(wsaIndex);
            else if (wsaNetEvents.lNetworkEvents == FD_CLOSE)
                DisconnectClient(wsaIndex);
        }
    }

    closesocket(listenSocket); // 리스닝 소켓 종료

    // 클라이언트 소켓 종료
    for (int i = 1; i <= MAX_PLAYERS; ++i) {
        if (clients[i]->clientSocket != INVALID_SOCKET) {
            closesocket(clients[i]->clientSocket);
        }
    }

    // Cleanup winsock <-> WSAStartup
    WSACleanup();

    for (auto& it : clients) {
        delete it.second;
        clients.erase(it.first);
    }
    return 0;
}

// clients, packetHandlers의 함수 포인터 초기화
void ChatServer::InitializeServerData() {
    for (int i = 1; i <= MAX_PLAYERS; i++) // 최대 플레이어 수 만큼 미리 clients 생성
    {
        clients[i] = new Client(i, chatServerHandle);
    }

    chatServerHandle->InitializePacketHandlers();

    cout << "Initialized packets." << endl;
}

void ChatServer::DisconnectClient(int index) {
    printf("%s:%d has disconnected.\n", clients[index]->ip_address, clients[index]->port);

    closesocket(clients[index]->clientSocket);
    clients[index]->clientSocket = INVALID_SOCKET;
    WSACloseEvent(handle_array[index]);

    if (index < total_socket_count - 1) // 이벤트 배열 정리
    {
        handle_array[index] = handle_array[total_socket_count - 1];
    }

    total_socket_count--;
}