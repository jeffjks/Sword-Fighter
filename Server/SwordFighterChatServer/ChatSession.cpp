#include "ChatSession.h"
#include "PacketDTO.h"
#include <iostream>

ChatSession::ChatSession(tcp::socket socket, ChatRoom& room, Dispatcher& dispatcher)
    : socket_(std::move(socket)), room_(room), dispatcher_(dispatcher) {}

void ChatSession::start() {
    try {
        std::cout << "[Info] "
            << socket_.remote_endpoint().address().to_string()
            << ":" << socket_.remote_endpoint().port() << " connected successfully." << std::endl;
    }
    catch (std::exception& e) {
        std::cout << "[Error] Failed to get ip adress: " << e.what() << "\n";
    }
    room_.join(shared_from_this());
    do_read();
}

void ChatSession::deliver(const int fromUserID, const std::string& msg) {
    bool write_in_progress = !write_msgs_.empty();

    ChatMessageRespDTO chatMessage(fromUserID, msg);
    std::string fullPacketStr = dispatcher_.makeNetworkPacket(ChatServerPackets::chatMessage, chatMessage);
    write_msgs_.push_back(fullPacketStr + "\n");

    if (!write_in_progress)
        do_write();
}

void ChatSession::set_userID(const int userID) {
    userID_ = userID;

    bool write_in_progress = !write_msgs_.empty();

    WelcomeRespDTO welcome;
    std::string fullPacketStr = dispatcher_.makeNetworkPacket(ChatServerPackets::welcome, welcome);
    write_msgs_.push_back(fullPacketStr + "\n");

    if (!write_in_progress)
        do_write();
}

void ChatSession::sendToRoom(const std::string& msg) {
    room_.broadcast(userID_, msg, shared_from_this());
    std::cout << "[Chat] " << userID_ << ": " << msg << std::endl;
}

void ChatSession::do_read() {
    auto self = shared_from_this();
    boost::asio::async_read_until(socket_, boost::asio::dynamic_buffer(read_msg_), '\n',
        [this, self](boost::system::error_code ec, std::size_t length) {
            if (!ec) {
                std::string msg = read_msg_.substr(0, length);
                //std::cout << msg << std::endl;
                read_msg_.erase(0, length);

                dispatcher_.dispatch(msg, self);

                do_read();
            }
            else {
                disconnect();
            }
        });
}

void ChatSession::do_write() {
    auto self(shared_from_this());

    boost::asio::async_write(socket_,
        boost::asio::buffer(write_msgs_.front()),
        [this, self](boost::system::error_code ec, std::size_t) {
            if (!ec) {
                write_msgs_.pop_front();
                if (!write_msgs_.empty())
                    do_write();
            }
            else {
                disconnect();
            }
        });
}

void ChatSession::disconnect()
{
    room_.leave(shared_from_this());
    std::cout << "[Info] "
        << socket_.remote_endpoint().address().to_string()
        << ":" << socket_.remote_endpoint().port() << " has disconnected." << std::endl;
}