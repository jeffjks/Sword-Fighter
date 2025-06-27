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

        const auto thread_num = std::thread::hardware_concurrency();

        std::cout << "[Info] Chat Server Started (Thread Number: " << thread_num << "). Listening on "
            << endpoint.address().to_string()
            << ":" << endpoint.port() << "\n";

        std::vector<std::thread> threads;

        for (std::size_t i = 0; i < thread_num; ++i) {
            threads.emplace_back([&io_context]() {
                io_context.run();
            });
        }

        for (auto& t : threads) {
            t.join();
        }
    }
    catch (std::exception& e) {
        std::cerr << "Exception: " << e.what() << "\n";
    }

    return 0;
}