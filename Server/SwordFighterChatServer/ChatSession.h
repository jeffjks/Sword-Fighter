#pragma once
#include "ChatParticipant.h"
#include "ChatRoom.h"
#include "Dispatcher.h"
#include <deque>
#include <string>
#include <memory>
#include <boost/asio.hpp>

class ChatSession;

using boost::asio::ip::tcp;

class ChatSession : public ChatParticipant, public std::enable_shared_from_this<ChatSession> {
public:
    ChatSession(tcp::socket socket, ChatRoom& room, Dispatcher& dispatcher);
    void start();
    void deliver(const std::string& msg) override;
    void broadcast(const std::string& msg);

private:
    void do_read();
    void do_write();
    void disconnect();

    tcp::socket socket_;
    ChatRoom& room_;
    Dispatcher& dispatcher_;
    std::string read_msg_;
    std::deque<std::string> write_msgs_;
};