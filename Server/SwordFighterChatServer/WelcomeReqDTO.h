#pragma once
#include <string>
#include "json.hpp"

struct WelcomeReqDTO {
    int UserID;

    WelcomeReqDTO() = default;

    WelcomeReqDTO(const int userId)
        : UserID(userId) {}
};

inline void to_json(nlohmann::json& j, const WelcomeReqDTO& msg) {
    j = nlohmann::json{ {"UserID", msg.UserID} };
}

inline void from_json(const nlohmann::json& j, WelcomeReqDTO& msg) {
    j.at("UserID").get_to(msg.UserID);
}