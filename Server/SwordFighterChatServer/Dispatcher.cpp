#include "Dispatcher.h"
#include "ChatSession.h"

Dispatcher::Dispatcher()
{
    setupHandlers();
}

void Dispatcher::dispatch(const std::string& msg, std::shared_ptr<ChatSession> session) {
    try {
        nlohmann::json jsonStr = nlohmann::json::parse(msg);
        ChatServerPackets type = static_cast<ChatServerPackets>(jsonStr["Type"].get<int>());

        auto it = handlerMap_.find(type);
        if (it != handlerMap_.end()) {
            it->second(jsonStr, session);
        }
        else {
            std::cerr << "Unknown message type: " << type << std::endl;
        }
    }
    catch (std::exception& e) {
        std::cerr << "JSON parse error: " << e.what() << std::endl;
    }
}

void Dispatcher::setupHandlers()
{
    handlerMap_[welcome] = [](const nlohmann::json& jsonStr, std::shared_ptr<ChatSession> session) {
        const auto& payloadJson = jsonStr["Payload"];
        WelcomeReqDTO welcomeReqDTO = payloadJson.get<WelcomeReqDTO>();
        const int userID = welcomeReqDTO.UserID;

        session->set_userID(userID);
        };

    handlerMap_[chatMessage] = [](const nlohmann::json& jsonStr, std::shared_ptr<ChatSession> session) {
        const auto& payloadJson = jsonStr["Payload"];
        ChatMessageReqDTO chatMessageReqDTO = payloadJson.get<ChatMessageReqDTO>();
        const std::string& msg = chatMessageReqDTO.Message;

        session->sendToRoom(msg);
        };
}