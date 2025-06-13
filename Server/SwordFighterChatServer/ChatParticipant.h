#pragma once
#include <string>

class ChatParticipant {
public:
    virtual ~ChatParticipant() = default;
    virtual void deliver(const int fromUserID, const std::string& msg) = 0;
};