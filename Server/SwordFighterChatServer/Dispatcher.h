#pragma once
#include <iostream>
#include <boost/asio.hpp>
#include "Packets.h"
#include "PacketDTO.h"
#include "NetworkDTO.h"
#include "json.hpp"

class ChatSession;

using HandlerFunc = std::function<void(const nlohmann::json&, std::shared_ptr<ChatSession>)>;

class Dispatcher {
public:
    Dispatcher();
    void dispatch(const std::string& msg, std::shared_ptr<ChatSession> session);

    template <typename T>
    std::string makeNetworkPacket(ChatServerPackets type, const T& payload) {
        NetworkDTO<T> networkDTO(type, payload);
        nlohmann::json j = networkDTO;

        return j.dump() + "\n";
    }

private:
    void setupHandlers();

    std::unordered_map<ChatServerPackets, HandlerFunc> handlerMap_;
};