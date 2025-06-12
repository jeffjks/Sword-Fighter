#include "HandlerMapManager.h"

void HandlerMapManager::setupHandlers() {
    handlerMap[chatMessage] = [](const nlohmann::json& j, std::shared_ptr<ChatSession> session) {
        std::cout << "[Chat] " << j["UserID"] << ": " << j["Message"] << std::endl;
        auto msg = j["Message"];
        session->broadcast(msg);
        };
}