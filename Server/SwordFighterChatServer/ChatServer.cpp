#include "ChatServer.h"

ChatServer::ChatServer(boost::asio::io_context& io_context, const tcp::endpoint& endpoint)
    : acceptor_(io_context, endpoint) {
    do_accept();
}

void ChatServer::do_accept() {
    acceptor_.async_accept(
        [this](boost::system::error_code ec, tcp::socket socket) {
            if (!ec) {
                std::make_shared<ChatSession>(std::move(socket), room_, dispatcher_)->start();
            }
            do_accept();
        });
}