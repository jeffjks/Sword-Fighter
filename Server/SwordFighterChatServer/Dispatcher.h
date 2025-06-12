#pragma once
#include <iostream>
#include <boost/asio.hpp>
#include "Packets.h"
#include "json.hpp"

class ChatSession;

using HandlerFunc = std::function<void(const nlohmann::json&, std::shared_ptr<ChatSession>)>;

class Dispatcher {
public:
    Dispatcher();
    void dispatch(const std::string& msg, std::shared_ptr<ChatSession> session);

private:
    void setupHandlers();

    std::unordered_map<ChatServerPackets, HandlerFunc> handlerMap_;
};