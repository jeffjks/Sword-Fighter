#include "ChatServer.h"
#include <boost/asio.hpp>
#include <iostream>
#include <memory>

using boost::asio::ip::tcp;

int main(int argc, char* argv[]) {
    SetConsoleOutputCP(CP_UTF8);

    try {
        if (argc < 2) {
            std::cerr << "Usage: chat_server <port>\n";
            return 1;
        }

        boost::asio::io_context io_context;
        tcp::endpoint endpoint(tcp::v4(), std::atoi(argv[1]));
        ChatServer server(io_context, endpoint);

        std::cout << "[Info] Chat Server Started. Listening on "
            << endpoint.address().to_string()
            << ":" << endpoint.port() << "\n";

        io_context.run();
    }
    catch (std::exception& e) {
        std::cerr << "Exception: " << e.what() << "\n";
    }

    return 0;
}