#include "ChatServer.h"

using namespace std;

void Client::ReceiveData() {
    // While loop: Ŭ���̾�Ʈ�� �޼����� �޾Ƽ� ��� �� Ŭ���̾�Ʈ�� �ٽ� �����ϴ�.
    char buf[dataBufferSize];

    // Wait for client to send data
    // �޼����� ���������� ������ recv �Լ��� �޼����� ũ�⸦ ��ȯ�Ѵ�.

    ZeroMemory(buf, dataBufferSize);

    int bytesReceived = recv(clientSocket, buf, dataBufferSize, 0);
    if (bytesReceived == SOCKET_ERROR)
    {
        cerr << "Error in recv(). Quitting" << endl;
        Disconnect();
        return;
    }
    else if (bytesReceived == 0)
    {
        cerr << "Client disconnected " << endl;
        Disconnect();
        return;
    }

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
        const char* packetBytes = receivedData.ReadBytes(packetLength); // receivedData���� packetLength��ŭ �� ����

        Packet packet(packetBytes, length);

        int packetId = packet.ReadInt() - 1;
        try {
            fp[packetId](index, packet);
        }
        catch (exception e) {
            cout << "Unknown packet id" << endl;
        }

        packetLength = 0;
        if (receivedData.UnreadLength() >= 4) // byte�� ���Ҵٸ� packetLength�� �а� �ٽ� �б� ����
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

void Client::Disconnect() {
    chatServer->DisconnectClient(index);
}