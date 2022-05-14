#include <stdio.h>
#include <stdlib.h>
#include <WinSock2.h>
#include <iostream>

using namespace std;

int const SERVER_PORT = 26950;

int main(int argc, char** argv)
{
	WSADATA wsaData;
	int iniResult = WSAStartup(MAKEWORD(2, 2), &wsaData); // MakeWord로 해당하는 버전의 Windows Sockets API 버전 초기화
	if (iniResult != 0)
	{
		cerr << "Can't Initialize winsock! Quitiing" << endl;
		return -1;
	}

	SOCKET listeningSock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP); // 3번 째 인자 0도 OK
	if (listeningSock == INVALID_SOCKET)
	{
		cerr << "Can't create a socket! Quitting" << endl;
		WSACleanup();
		return -1;
	}

	sockaddr_in hint{}; // 기본 초기화 권장
	hint.sin_family = AF_INET;
	hint.sin_addr.S_un.S_addr = htonl(INADDR_ANY);
	hint.sin_port = htons(SERVER_PORT);

	int bindResult = bind(listeningSock, reinterpret_cast<sockaddr*>(&hint), sizeof(hint));
	if (bindResult == SOCKET_ERROR)
	{
		cerr << "Can't bind a socket! Quitting" << endl;
		closesocket(listeningSock);
		WSACleanup();
		return -1;
	}

	int listenResult = listen(listeningSock, SOMAXCONN);
	if (listenResult == SOCKET_ERROR)
	{
		cerr << "Can't listen a socket! Quitting" << endl;
		closesocket(listeningSock);
		WSACleanup();
		return -1;
	}

	sockaddr_in clientSockInfo;
	int clientSize = sizeof(clientSockInfo);

	// connection queue의 가장 앞에 있는 클라이언트 요청을 accept하고, client 소켓을 반환합니다.
	SOCKET clientSocket = accept(listeningSock, reinterpret_cast<sockaddr*>(&clientSockInfo), &clientSize);
	if (clientSocket == INVALID_SOCKET)
	{
		cerr << "Can't accept a socket! Quitting" << endl;
		closesocket(listeningSock);
		WSACleanup();
		return -1;
	}

	// close listening socket
	int closeResult = closesocket(listeningSock);

	char host[NI_MAXHOST];	         // 클라이언트의 host 이름
	char service[NI_MAXHOST];        // 클라이언트의 PORT 번호
	ZeroMemory(host, NI_MAXHOST);    // memset(host, 0, NI_MAXHOST)와 동일
	ZeroMemory(service, NI_MAXHOST);

	// clientSockInfo에 저장된 IP 주소를 통해 도메인 정보를 얻습니다. host 이름은 host에, 포트 번호는 service에 저장됩니다.
	// getnameinfo()는 성공 시 0을 반환합니다. 실패 시 0이 아닌 값을 반환합니다.
	if (getnameinfo((sockaddr*)&clientSockInfo, sizeof(clientSockInfo), host, NI_MAXHOST, service, NI_MAXSERV, 0) == 0)
	{
		cout << host << " connected ON port " << service << endl;
	}
	else
	{
		inet_ntop(AF_INET, &clientSockInfo.sin_addr, host, NI_MAXHOST);
		cout << host << " connected on port " << ntohs(clientSockInfo.sin_port) << endl;
	}


	// While loop: 클라이언트의 메세지를 받아서 출력 후 클라이언트에 다시 보냅니다.
	enum eBufSize { BUF_SIZE = 4096 };
	char buf[BUF_SIZE];

	while (true)
	{
		ZeroMemory(buf, BUF_SIZE);

		// Wait for client to send data
		// 메세지를 성공적으로 받으면 recv 함수는 메세지의 크기를 반환한다.
		int bytesReceived = recv(clientSocket, buf, BUF_SIZE, 0);
		if (bytesReceived == SOCKET_ERROR)
		{
			cerr << "Error in recv(). Quitting" << endl;
			break;
		}

		if (bytesReceived == 0)
		{
			cout << "Client disconnected " << endl;
			break;
		}

		// Echo message back to client
		cout << buf << endl;
		send(clientSocket, buf, bytesReceived + 1, 0);
	}
}

class Program {

};