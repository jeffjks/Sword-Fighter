#pragma once
#include <string>
#include "json.hpp"

struct ChatMessageRespDTO {
    int UserID;
    std::string Message;

    ChatMessageRespDTO() = default;

    ChatMessageRespDTO(const int userId, const std::string& message)
        : UserID(userId), Message(message) {}
};

inline void to_json(nlohmann::json& j, const ChatMessageRespDTO& msg) {
    j = nlohmann::json{ {"UserID", msg.UserID}, {"Message", msg.Message} };
}

inline void from_json(const nlohmann::json& j, ChatMessageRespDTO& msg) {
    j.at("UserID").get_to(msg.UserID);
    j.at("Message").get_to(msg.Message);
}