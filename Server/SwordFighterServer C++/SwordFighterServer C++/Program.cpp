#include "ChatServer.h"

using namespace std;

// https://mawile.tistory.com/40
// https://twinparadox.tistory.com/205



int main() {
    #ifdef _WIN32
        SetConsoleOutputCP(CP_UTF8);
    #endif

    ChatServer chatServer;
    chatServer.Start();
}


/*
void proc_recvs() {
    char buffer[BUFFER_SIZE] = { 0 };

    while (!WSAGetLastError()) {
        ZeroMemory(&buffer, BUFFER_SIZE); // 메모리 영역을 0으로 채우는 매크로
        recv(client_sock, buffer, BUFFER_SIZE, 0);
        cout << "받은 메세지: " << buffer << endl;
    }
}

int main() {

    // 소켓 라이브러리 초기화
    WSADATA wsaData;
    WSAStartup(WINSOCK_VERSION, &wsaData);

    // 소켓 배열   다중 클라이언트 접속을 하기위해 배열을 사용.
    SOCKET socket_arry[MAX_PLAYER] = { 0 };   //최대값은 위에서 정의해줌.
    HANDLE hEvent_arry[MAX_PLAYER] = { 0 };

    // 대기용 소켓 생성
    socket_main = socket(AF_INET, SOCK_STREAM, 0);

    // 소켓 주소 정보 작성
    SOCKADDR_IN servAddr;
    servAddr.sin_family = AF_INET;
    servAddr.sin_addr.s_addr = htonl(INADDR_ANY);
    servAddr.sin_port = htons(PORT); // 포트 번호를 받아서 사용.

    // 소켓 바인드
    bind(socket_main, (SOCKADDR*)&servAddr, sizeof(servAddr));

    // 소켓 대기
    if (listen(socket_main, SOMAXCONN) == SOCKET_ERROR)
    {
        closesocket(socket_main);
        return -1;
    }

    SOCKADDR_IN client = {};
    int client_size = sizeof(client);
    ZeroMemory(&client, client_size);
    client_sock = accept(socket_main, (SOCKADDR*)&client, &client_size);

    char buffer[BUFFER_SIZE] = { 0 };
    thread proc2(proc_recvs);

    while (!WSAGetLastError()) {
        cin >> buffer;
        send(client_sock, buffer, strlen(buffer), 0);
    }
    proc2.join();
    closesocket(client_sock);
    closesocket(socket_main);
    WSACleanup();
}

/*
#include <winsock2.h>
#include <stdio.h>
#include <stdlib.h>
#include <iostream>

const int MAX_PLAYER = 4;
const int PORT = 26960;
const int BUFFER_SIZE = 4096;

// https://ehpub.co.kr/5-1-%EC%B1%84%ED%8C%85-%EC%84%9C%EB%B2%84-%EA%B5%AC%ED%98%84-tcpip-%EC%86%8C%EC%BC%93-%ED%94%84%EB%A1%9C%EA%B7%B8%EB%9E%98%EB%B0%8D-with-%EC%9C%88%EB%8F%84%EC%9A%B0%EC%A6%88/

int main()
{
    // 소켓 라이브러리 초기화
    WSADATA wsaData;
    int token = WSAStartup(WINSOCK_VERSION, &wsaData);

    // 소켓 배열   다중 클라이언트 접속을 하기위해 배열을 사용.
    SOCKET socket_arry[MAX_PLAYER] = { 0 };   //최대값은 위에서 정의해줌.
    HANDLE hEvent_arry[MAX_PLAYER] = { 0 };

    // 대기용 소켓 생성
    socket_arry[0] = socket(AF_INET, SOCK_STREAM, 0);

    // 소켓 주소 정보 작성
    SOCKADDR_IN servAddr;
    servAddr.sin_family = AF_INET;
    servAddr.sin_addr.s_addr = htonl(INADDR_ANY);
    servAddr.sin_port = htons(PORT); // 포트 번호를 받아서 사용.

    // 소켓 바인드
    if (bind(socket_arry[0], (sockaddr *)&servAddr, sizeof(servAddr)) == SOCKET_ERROR)
    {
        closesocket(socket_arry[0]);
        return -1;
    }

    // 소켓 대기
    if (listen(socket_arry[0], SOMAXCONN) == SOCKET_ERROR)
    {
        closesocket(socket_arry[0]);
        return -1;
    }

    // 이벤트 핸들 생성
    for (int i = 0; i < MAX_PLAYER; i++)
    {
        hEvent_arry[i] = CreateEvent(NULL, FALSE, FALSE, NULL);
        if (hEvent_arry[i] == INVALID_HANDLE_VALUE)
        {
            closesocket(socket_arry[0]);
            std::cout << "Failed to Create event." << std::endl;
            return SOCKET_ERROR;
        }
    }

    // 대기 소켓 이벤트 핸들 설정
    if (WSAEventSelect(socket_arry[0], hEvent_arry[0], FD_ACCEPT) == SOCKET_ERROR)
    {
        closesocket(socket_arry[0]);
        CloseHandle(hEvent_arry[0]);
        std::cout << "Failed to Event select." << std::endl;
        return SOCKET_ERROR;
    }

    // 상태 출력
    DWORD dwTmp;
    std::cout << "Server started on " << PORT << "." << std::endl;

    // 설정된 이벤트 핸들 갯수
    int client = 1;

    // 접속 루프
    while (true)
    {
        // 소켓 접속 대기
        DWORD dwObject = WaitForMultipleObjectsEx(client, hEvent_arry, FALSE, INFINITE, 0);
        if (dwObject == INFINITE)
            continue;

        if (dwObject == WAIT_OBJECT_0)
        {
            // 클라이언트 소켓 생성
            SOCKADDR_IN clntAddr;
            int clntLen = sizeof(clntAddr);
            SOCKET socket_client = accept(socket_arry[0], (SOCKADDR*)&clntAddr, &clntLen);

            // 빈 배열 검색
            int index = -1;
            for (int c = 1; c < MAX_PLAYER; c++)
            {
                if (socket_arry[c] == 0)
                {
                    index = c;
                    break;
                }
            }

            if (index > 0)  //하나라도 접속
            {
                // 클라이언트 이벤트 핸들 설정
                if (WSAEventSelect(socket_client, hEvent_arry[index], FD_READ | FD_CLOSE) == SOCKET_ERROR)
                {
                    DWORD dwTmp;
                    std::cout << "Failed to Event select." << std::endl;
                    continue;
                }

                char buffer[BUFFER_SIZE] = { 0 };

                std::cout << "Incoming connection from " << index << std::endl;

                // 배열에 소켓 저장
                socket_arry[index] = socket_client;
                client = max(client, index + 1);

                char send_buffer[BUFFER_SIZE] = { 0 };
                wsprintfA(send_buffer, "1Test");
                send(socket_arry[index], send_buffer, strlen(send_buffer), 0);
                std::cout << "send Test" << std::endl;
            }
            else  //허용 소켓 초과
            {
                DWORD dwTmp;
                std::cout << "Failed to connect: Server full!" << std::endl;
                closesocket(socket_client);
            }
        }
        else
        {
            int index = (dwObject - WAIT_OBJECT_0);

            DWORD dwTmp;
            char buffer[BUFFER_SIZE] = { 0 };
            wsprintfA(buffer, "%d번 사용자 : ", index);
            WriteFile(GetStdHandle(STD_OUTPUT_HANDLE), buffer, strlen(buffer), &dwTmp, NULL);



            // 메시지 수신
            memset(buffer, 0, sizeof(buffer));
            int cnt = recv(socket_arry[index], buffer, sizeof(buffer), 0);

            if (cnt <= 0)
            {
                // 클라이언트 접속 종료
                closesocket(socket_arry[index]);
                std::cout << index << " has disconnected" << std::endl;

                // 배열에 소켓 제거
                socket_arry[index] = 0;
                continue;
            }
            // 메시지 출력
            WriteFile(GetStdHandle(STD_OUTPUT_HANDLE), buffer, cnt, &dwTmp, NULL);

            // 애코 처리
            char send_buffer[BUFFER_SIZE] = { 0 };
            wsprintfA(send_buffer, "%d 번 사용자 : %s", index, buffer);

            for (int c = 1; c < MAX_PLAYER; c++)
            {
                if (socket_arry[c] != 0 && c != index)
                {
                    // 수신받은 클라이언트 제외 하고 애코 처리
                    send(socket_arry[c], send_buffer, strlen(send_buffer), 0);
                }
            }
        }

    }
    // 서버 소켓 해제
    closesocket(socket_arry[0]);
    CloseHandle(hEvent_arry[0]);
    return 0;
}
*/