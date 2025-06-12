#pragma once
#include "ChatSession.h"
#include "Packets.h"
#include "json.hpp"
#include <boost/asio.hpp>
#include <unordered_map>
#include <iostream>
#include <memory>

using boost::asio::ip::tcp;

class ChatServer {
public:
    ChatServer(boost::asio::io_context& io_context, const tcp::endpoint& endpoint);

private:
    void do_accept();

    tcp::acceptor acceptor_;
    ChatRoom room_;
    Dispatcher dispatcher_;
};