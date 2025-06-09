#include <boost/asio.hpp>
#include <iostream>
#include <memory>
#include <set>
#include <deque>

using boost::asio::ip::tcp;

class ChatParticipant {
public:
    virtual ~ChatParticipant() {}
    virtual void deliver(const std::string& msg) = 0;
};

using ChatParticipantPtr = std::shared_ptr<ChatParticipant>;

class ChatRoom {
public:
    void join(ChatParticipantPtr participant) {
        participants.insert(participant);
    }

    void leave(ChatParticipantPtr participant) {
        participants.erase(participant);
    }

    void broadcast(const std::string& msg) {
        for (auto& p : participants)
            p->deliver(msg);
    }

    void broadcast(const std::string& msg, ChatParticipantPtr participant) {
        for (auto& p : participants)
        {
            if (p == participant)
                continue;
            p->deliver(msg);
        }
    }

private:
    std::set<ChatParticipantPtr> participants;
};

class ChatSession : public ChatParticipant, public std::enable_shared_from_this<ChatSession> {
public:
    ChatSession(tcp::socket socket, ChatRoom& room)
        : socket_(std::move(socket)), room_(room) {}

    void start() {
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

    void deliver(const std::string& msg) override {
        bool write_in_progress = !write_msgs_.empty();
        write_msgs_.push_back(msg);
        if (!write_in_progress)
            do_write();
    }

private:
    void do_read() {
        auto self = shared_from_this();
        boost::asio::async_read_until(socket_, boost::asio::dynamic_buffer(read_msg_), '\n',
            [this, self](boost::system::error_code ec, std::size_t length) {
                if (!ec) {
                    std::string msg = read_msg_.substr(0, length);
                    std::cout << msg << std::endl;
                    read_msg_.erase(0, length);
                    room_.broadcast(msg, self);
                    do_read();
                }
                else {
                    disconnect();
                }
            });
    }

    void do_write() {
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

    void disconnect()
    {
        room_.leave(shared_from_this());
        std::cout << "[Info] "
            << socket_.remote_endpoint().address().to_string()
            << ":" << socket_.remote_endpoint().port() << " has disconnected." << std::endl;
    }

    tcp::socket socket_;
    ChatRoom& room_;
    std::string read_msg_;
    std::deque<std::string> write_msgs_;
};

class ChatServer {
public:
    ChatServer(boost::asio::io_context& io_context, const tcp::endpoint& endpoint)
        : acceptor_(io_context, endpoint) {
        do_accept();
    }

private:
    void do_accept() {
        acceptor_.async_accept(
            [this](boost::system::error_code ec, tcp::socket socket) {
                if (!ec) {
                    std::make_shared<ChatSession>(std::move(socket), room_)->start();
                }
                do_accept();
            });
    }

    tcp::acceptor acceptor_;
    ChatRoom room_;
};

int main(int argc, char* argv[]) {
    try {
        if (argc < 2) {
            std::cerr << "Usage: chat_server <port>\n";
            return 1;
        }

        boost::asio::io_context io_context;
        tcp::endpoint endpoint(tcp::v4(), std::atoi(argv[1]));
        ChatServer server(io_context, endpoint);

        std::cout << "[Info] Server Started. Listening on "
            << endpoint.address().to_string()
            << ":" << endpoint.port() << "\n";

        io_context.run();
    }
    catch (std::exception& e) {
        std::cerr << "Exception: " << e.what() << "\n";
    }

    return 0;
}