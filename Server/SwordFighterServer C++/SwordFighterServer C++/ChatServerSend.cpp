
class ChatServerSend {

public:
	void SendTCPData(int toClient, Packet packet);
};


void ChatServerSend::SendTCPData(int toClient, Packet packet)
{
	packet.WriteLength();
	Server.clients[toClient].tcp.SendData(packet);
}