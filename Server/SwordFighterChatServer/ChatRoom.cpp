#include "ChatRoom.h"

void ChatRoom::join(std::shared_ptr<ChatParticipant> participant) {
    participants.insert(participant);
}

void ChatRoom::leave(std::shared_ptr<ChatParticipant> participant) {
    participants.erase(participant);
}

void ChatRoom::broadcast(const std::string& msg) {
    for (auto& p : participants)
        p->deliver(msg);
}

void ChatRoom::broadcast(const std::string& msg, std::shared_ptr<ChatParticipant> participant) {
    for (auto& p : participants)
    {
        if (p == participant)
            continue;
        p->deliver(msg);
    }
}