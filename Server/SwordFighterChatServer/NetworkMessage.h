#pragma once
#include <string>
#include "json.hpp"

template <typename T>
struct NetworkMessage {
    ChatServerPackets Type;
    T Payload;
};

template <typename T>
void to_json(nlohmann::json& j, const NetworkMessage<T>& msg) {
    j = nlohmann::json{
        {"Type", static_cast<int>(msg.Type)},
        {"Payload", msg.Payload}
    };
}

template <typename T>
void from_json(const nlohmann::json& j, NetworkMessage<T>& msg) {
    int typeInt;
    j.at("Type").get_to(typeInt);
    msg.Type = static_cast<ChatServerPackets>(typeInt);
    j.at("Payload").get_to(msg.Payload);
}