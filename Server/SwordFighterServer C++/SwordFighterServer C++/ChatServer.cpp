#include "ChatServer.h"

using namespace std;

/*
    Server
*/

void ChatServer::Broadcast() {
    while (true) {
        if (messageQueue.empty()) {
            continue;
        }
        mtx.lock();
        while (!messageQueue.empty()) {
            pair<int, string> msg = messageQueue.front();
            ChatMessage(msg.first, msg.second);
            messageQueue.pop();
        }
        mtx.unlock();
    }
}

void ChatServer::SendTCPDataToAll(int fromId, Packet packet, bool exceptMe = false) {
    const char* send_buffer = packet.ToArray();

    for (int i = 1; i <= MAX_PLAYERS; ++i) {
        if (clients[i]->clientSocket == INVALID_SOCKET) {
            continue;
        }
        if (fromId == i && exceptMe) {
            continue;
        }
        cout << i << ", " << fromId << ", " << exceptMe << endl;
        send(clients[i]->clientSocket, send_buffer, packet.Length(), 0);
    }
}

void ChatServer::ChatMessage(int fromId, string str) {
    Packet packet;
    packet.Write(fromId); // 채팅 주인
    packet.Write(str); // 채팅 내용
    packet.WriteLength(); // 패킷 길이
    cout << fromId << ": " << str << endl;

    SendTCPDataToAll(fromId, packet, true);
}

void ChatServer::InitializeServerData()
{
    for (int i = 1; i <= MAX_PLAYERS; i++) // 최대 플레이어 수 만큼 미리 clients 생성
    {
        clients[i] = new Client(i, this);
    }
}

void ChatServer::AcceptClient(int index) {
    int key;
    if (wsaNetEvents.iErrorCode[FD_ACCEPT_BIT] != 0)
    {
        // 에러 로그를 출력하고 다음 순서를 진행한다.
        cout << "Accept Error !!" << endl;
        return;
    }
    if (total_socket_count == FD_SETSIZE) {
        return;
    }

    SOCKADDR_IN acceptClientSockaddr;
    int clientSize = sizeof(SOCKADDR_IN);
    // connection queue의 가장 앞에 있는 클라이언트 요청을 accept하고, client 소켓을 반환합니다.
    SOCKET acceptClientSocket = accept(listenSocket, (SOCKADDR*)& acceptClientSockaddr, &clientSize); // accept한 클라이언트는 clientSocket으로 통신

    if (acceptClientSocket == INVALID_SOCKET)
    {
        cerr << "Can't accept a socket! Quitting" << endl;
        closesocket(listenSocket);
        WSACleanup();
        return;
    }

    //strcpy_s(sock_array[total_socket_count].ipaddr, inet_ntoa(addr.sin_addr));


    for (int i = 1; i <= MAX_PLAYERS; i++) // 비어있는 가장 첫 clients Dictionary에 배정
    {
        if (clients[i]->clientSocket == INVALID_SOCKET)
        {
            HANDLE wsaEvent = WSACreateEvent();

            clients[i]->clientSocket = acceptClientSocket;
            clients[i]->evnt = wsaEvent;

            WSAEventSelect(acceptClientSocket, wsaEvent, FD_READ | FD_CLOSE);
            total_socket_count++;
            key = i;
            cout << "key: " << key << endl;
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
    if (getnameinfo((const SOCKADDR*)&acceptClientSockaddr, sizeof(SOCKADDR_IN), host, NI_MAXHOST, service, NI_MAXSERV, 0) == 0)
    {
        cout << host << " connected on port " << service << endl;
    }
    else
    {
        inet_ntop(AF_INET, &acceptClientSockaddr.sin_addr, host, NI_MAXHOST);
        cout << host << " connected on port " << ntohs(acceptClientSockaddr.sin_port) << endl;
    }


    Packet packet;
    string str = string(u8"환영합니다.");

    packet.Write(SERVER_MESSAGE);
    packet.Write(str);
    packet.WriteLength();

    const char* send_buffer = packet.ToArray();

    send(clients[key]->clientSocket, send_buffer, packet.Length(), 0);

}

void ChatServer::ReceiveClientsData(int index) {
    if (wsaNetEvents.iErrorCode[FD_READ_BIT] != 0)
    {
        cout << "Recv Error !!" << endl;
        return;
    }
    clients[index]->ReceiveData();
    return;
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

    SOCKADDR_IN servAddr; // 기본 초기화 권장
    servAddr.sin_family = AF_INET;
    servAddr.sin_addr.S_un.S_addr = htonl(INADDR_ANY);
    servAddr.sin_port = htons(PORT); // short 자료형을 네트워크 byte order로 변경 (Big Endian)

    int bindResult = ::bind(listenSocket, reinterpret_cast<sockaddr*>(&servAddr), sizeof(servAddr));
    if (bindResult == SOCKET_ERROR)
    {
        cerr << "Can't bind a socket! Quitting" << endl;
        closesocket(listenSocket);
        WSACleanup();
        return -2;
    }

    cout << "Starting Server..." << endl;
    int listenResult = listen(listenSocket, SOMAXCONN);
    if (listenResult == SOCKET_ERROR)
    {
        cerr << "Can't listen a socket! Quitting" << endl;
        closesocket(listenSocket);
        WSACleanup();
        return -3;
    }

    WSAEVENT wsaEvent = WSACreateEvent();
    clients[0] = new Client(listenSocket, wsaEvent, this);

    WSAEventSelect(listenSocket, wsaEvent, FD_ACCEPT);
    total_socket_count++;

    InitializeServerData();

    thread(&ChatServer::Broadcast, this).detach();


    while (true) {
        memset(&handle_array, 0, sizeof(handle_array));
        int num = 0;
        for (int i = 0; i <= MAX_PLAYERS; i++) {
            if (clients[i]->evnt == NULL) {
                continue;
            }
            handle_array[num] = clients[i]->evnt;
            num++;
        }

        index = WSAWaitForMultipleEvents(total_socket_count, handle_array, false, INFINITE, false);

        if ((index != WSA_WAIT_FAILED) && (index != WSA_WAIT_TIMEOUT))
        {
            WSAEnumNetworkEvents(clients[index]->clientSocket, clients[index]->evnt, &wsaNetEvents);
            if (wsaNetEvents.lNetworkEvents == FD_ACCEPT)
                AcceptClient(index);
            else if (wsaNetEvents.lNetworkEvents == FD_READ)
                ReceiveClientsData(index);
            else if (wsaNetEvents.lNetworkEvents == FD_CLOSE)
                DisconnectClient(index);
        }
    }
    closesocket(listenSocket);


    // Close the client socket
    for (int i = 1; i <= MAX_PLAYERS; ++i) {
        if (clients[i]->clientSocket != INVALID_SOCKET) {
            closesocket(clients[i]->clientSocket);
        }
    }

    // Cleanup winsock <-> WSAStartup
    WSACleanup();

    delete *clients;
    return 0;
}

void ChatServer::DisconnectClient(int id) {
    closesocket(clients[id]->clientSocket);
    clients[id]->clientSocket = INVALID_SOCKET;
    clients[id]->evnt = NULL;
    WSACloseEvent(clients[id]->evnt);

    total_socket_count--;
}