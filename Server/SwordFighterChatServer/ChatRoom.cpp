#include "ChatRoom.h"

void ChatRoom::join(std::shared_ptr<ChatParticipant> participant) {
    participants.insert(participant);
}

void ChatRoom::leave(std::shared_ptr<ChatParticipant> participant) {
    participants.erase(participant);
}

void ChatRoom::broadcast(const int fromUserID, const std::string& msg) {
    for (auto& p : participants)
        p->deliver(fromUserID, msg);
}

void ChatRoom::broadcast(const int fromUserID, const std::string& msg, std::shared_ptr<ChatParticipant> participant) {
    for (auto& p : participants)
    {
        if (p == participant)
            continue;
        p->deliver(fromUserID, msg);
    }
}