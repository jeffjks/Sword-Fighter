#pragma once
#include "Client.h"
#include "ChatServerHandle.h"

void Client::SetUserData(int _id, string _username) {
    id = _id;
    username = _username;
}

// 패킷 RECV
void Client::ReceiveData() {
    char buf[dataBufferSize];

    ZeroMemory(buf, dataBufferSize);

    int bytesReceived = recv(clientSocket, buf, dataBufferSize, 0);

    Packet packet = receivedData;
    packet.Reset(HandleData(buf, bytesReceived));
}

bool Client::HandleData(char* data, int length) {
    int packetLength = 0;
    receivedData.SetBytes(data, length);

    if (receivedData.UnreadLength() >= 4) { // 패킷 총 길이 읽기
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
            // 함수 포인터를 사용하여 packetId에 따라 chatServer의 적절한 함수 실행
            chatServerHandle->HandlePacketId(packetId, index, packet);
            //(chatServerHandle->*(chatServerHandle->packetHandlers[packetId]))(index, packet);
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