#pragma once
#include "ChatServer.h"

using namespace std;

void Client::ReceiveData() {
    // While loop: 클라이언트의 메세지를 받아서 출력 후 클라이언트에 다시 보냅니다.
    char buf[dataBufferSize];

    // Wait for client to send data
    // 메세지를 성공적으로 받으면 recv 함수는 메세지의 크기를 반환한다.

    ZeroMemory(buf, dataBufferSize);

    int bytesReceived = recv(clientSocket, buf, dataBufferSize, 0);

    Packet packet = receivedData;
    packet.Reset(HandleData(buf, bytesReceived));
}

bool Client::HandleData(char* data, int length) {
    int packetLength = 0;
    receivedData.SetBytes(data, length);

    if (receivedData.UnreadLength() >= 4) {
        packetLength = receivedData.ReadInt();
        if (packetLength <= 0)
        {
            return true;
        }
    }

    while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
    {
        const char* packetBytes = receivedData.ReadBytes(packetLength); // receivedData에서 packetLength만큼 다 읽음

        Packet packet(packetBytes, length);

        int packetId = packet.ReadInt();
        try {
            // Pointer
            (chatServer->chatServerHandle.*(chatServer->packetHandlers[packetId]))(index, packet);

            //chatServer->*packetHandlers[packetId](index, packet);
        }
        catch (exception e) {
            cout << "Unknown packet id" << endl;
        }

        packetLength = 0;
        if (receivedData.UnreadLength() >= 4) // byte가 남았다면 packetLength를 읽고 다시 읽기 진행
        {
            packetLength = receivedData.ReadInt();
            if (packetLength <= 0)
            {
                return true;
            }
        }
    }

    if (packetLength <= 1)
    {
        return true;
    }

    return false;
}