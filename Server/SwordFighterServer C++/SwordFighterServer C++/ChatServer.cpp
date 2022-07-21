#include "ChatServer.h"

using namespace std;

/*
    Server
*/

void ChatServer::Broadcast() {
    while (true) {
        // TODO - Mutext 사용?

        //mtx.lock();
        while (!messageQueue.empty()) {
            pair<int, string> msg = messageQueue.front();
            cout << msg.first << ": " << msg.second << endl;
            messageQueue.pop();
        }
        //mtx.unlock();
    }
}

void ChatServer::InitializeServerData()
{
    for (int i = 0; i < MAX_PLAYERS; i++) // 최대 플레이어 수 만큼 미리 clients 생성
    {
        clients[i] = new Client(i);
    }
}

void ChatServer::AcceptClient() {
    while (true) {
        int key;
        SOCKADDR_IN acceptedClientSockaddr;
        int clientSize = sizeof(SOCKADDR_IN);
        // connection queue의 가장 앞에 있는 클라이언트 요청을 accept하고, client 소켓을 반환합니다.
        SOCKET acceptedClientSocket = accept(listenSocket, (SOCKADDR*)& acceptedClientSockaddr, &clientSize); // accept한 클라이언트는 clientSocket으로 통신

        if (acceptedClientSocket == INVALID_SOCKET)
        {
            cerr << "Can't accept a socket! Quitting" << endl;
            closesocket(listenSocket);
            WSACleanup();
            return;
        }

        for (int i = 0; i < MAX_PLAYERS; i++) // 비어있는 가장 첫 clients Dictionary에 배정
        {
            if (clients[i]->clientSocket == INVALID_SOCKET)
            {
                clients[i]->clientSocket = acceptedClientSocket;
                key = i;
                break;
            }
        }

        // close listening socket
        //int closeResult = closesocket(listenSocket);

        char host[NI_MAXHOST];             // 클라이언트의 host 이름
        char service[NI_MAXHOST];        // 클라이언트의 PORT 번호
        ZeroMemory(host, NI_MAXHOST);    // memset(host, 0, NI_MAXHOST)와 동일
        ZeroMemory(service, NI_MAXHOST);

        // clientAddr에 저장된 IP 주소를 통해 도메인 정보를 얻습니다. host 이름은 host에, 포트 번호는 service에 저장됩니다.
        // getnameinfo()는 성공 시 0을 반환합니다. 실패 시 0이 아닌 값을 반환합니다.
        if (getnameinfo((const SOCKADDR*)&acceptedClientSockaddr, sizeof(SOCKADDR_IN), host, NI_MAXHOST, service, NI_MAXSERV, 0) == 0)
        {
            cout << host << " connected on port " << service << endl;
        }
        else
        {
            inet_ntop(AF_INET, &acceptedClientSockaddr.sin_addr, host, NI_MAXHOST);
            cout << host << " connected on port " << ntohs(acceptedClientSockaddr.sin_port) << endl;
        }

        Packet packet;
        string str = string(u8"환영합니다.");

        packet.Write(SERVER_MESSAGE);
        packet.Write(str);
        packet.WriteLength();

        const char* send_buffer = packet.ToArray();

        send(clients[key]->clientSocket, send_buffer, packet.Length(), 0);

        clients[key]->Connect();
    }
}

int ChatServer::Start() {
    WSADATA wsaData;
    int iniResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
    if (iniResult != 0)
    {
        cerr << "Can't Initialize winsock! Quitiing" << endl;
        return -1;
    }

    listenSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP); // 리스닝 모드 소켓
    if (listenSocket == INVALID_SOCKET)
    {
        cerr << "Can't create a socket! Quitting" << endl;
        WSACleanup();
        return -1;
    }

    sockaddr_in servAddr{}; // 기본 초기화 권장
    servAddr.sin_family = AF_INET;
    servAddr.sin_addr.S_un.S_addr = htonl(INADDR_ANY);
    servAddr.sin_port = htons(PORT); // short 자료형을 네트워크 byte order로 변경 (Big Endian)

    int bindResult = ::bind(listenSocket, reinterpret_cast<sockaddr*>(&servAddr), sizeof(servAddr));
    if (bindResult == SOCKET_ERROR)
    {
        cerr << "Can't bind a socket! Quitting" << endl;
        closesocket(listenSocket);
        WSACleanup();
        return -1;
    }

    cout << "Starting Server..." << endl;
    int listenResult = listen(listenSocket, SOMAXCONN);
    if (listenResult == SOCKET_ERROR)
    {
        cerr << "Can't listen a socket! Quitting" << endl;
        closesocket(listenSocket);
        WSACleanup();
        return -1;
    }
    InitializeServerData();

    thread(Broadcast).detach();

    AcceptClient();


    while (true) {
        // 종료 방지
    }


    // Close the client socket
    for (int i = 0; i < MAX_PLAYERS; ++i) {
        if (clients[i]->clientSocket != INVALID_SOCKET) {
            closesocket(clients[i]->clientSocket);
        }
    }

    // Cleanup winsock <-> WSAStartup
    WSACleanup();

    delete *clients;
}