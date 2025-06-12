#pragma once
#include <string>

class ChatParticipant {
public:
    virtual ~ChatParticipant() = default;
    virtual void deliver(const std::string& msg) = 0;
};