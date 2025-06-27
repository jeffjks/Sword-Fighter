#include "ChatRoom.h"
#include <mutex>

void ChatRoom::join(std::shared_ptr<ChatParticipant> participant) {
    std::lock_guard<std::mutex> lock(mtx_);
    participants.insert(participant);
}

void ChatRoom::leave(std::shared_ptr<ChatParticipant> participant) {
    std::lock_guard<std::mutex> lock(mtx_);
    participants.erase(participant);
}

void ChatRoom::broadcast(const int fromUserID, const std::string& msg) {
    std::lock_guard<std::mutex> lock(mtx_);
    for (auto& p : participants)
        p->deliver(fromUserID, msg);
}

void ChatRoom::broadcast(const int fromUserID, const std::string& msg, std::shared_ptr<ChatParticipant> participant) {
    std::lock_guard<std::mutex> lock(mtx_);
    for (auto& p : participants)
    {
        if (p == participant)
            continue;
        p->deliver(fromUserID, msg);
    }
}