#pragma once
#include <iostream>
#include <unordered_map>
#include "json.hpp"
#include "ChatSession.h"
#include "Packets.h"

class HandlerMapManager {
public:
    using HandlerFunc = std::function<void(const nlohmann::json&, std::shared_ptr<ChatSession>)>;
    std::unordered_map<ChatServerPackets, HandlerFunc> handlerMap;

    void setupHandlers();
};