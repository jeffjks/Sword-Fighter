#pragma once
#include <string>
#include "json.hpp"

struct ChatMessage {
    int UserID;
    std::string Message;
};

void to_json(nlohmann::json& j, const ChatMessage& msg) {
    j = nlohmann::json{ {"UserID", msg.UserID}, {"Message", msg.Message} };
}

void from_json(const nlohmann::json& j, ChatMessage& msg) {
    j.at("UserID").get_to(msg.UserID);
    j.at("Message").get_to(msg.Message);
}