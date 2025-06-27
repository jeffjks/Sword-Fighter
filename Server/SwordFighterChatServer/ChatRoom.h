#pragma once
#include "ChatParticipant.h"
#include <boost/asio.hpp>
#include <memory>
#include <set>

using boost::asio::ip::tcp;

class ChatRoom {
public:
    void join(std::shared_ptr<ChatParticipant> participant);
    void leave(std::shared_ptr<ChatParticipant> participant);
    void broadcast(const int fromUserID, const std::string& msg);
    void broadcast(const int fromUserID, const std::string& msg, std::shared_ptr<ChatParticipant> participant);

private:
    std::set<std::shared_ptr<ChatParticipant>> participants;
    std::mutex mtx_;
};