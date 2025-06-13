#pragma once
#include <string>
#include "json.hpp"

struct WelcomeRespDTO {

    WelcomeRespDTO() = default;
};

inline void to_json(nlohmann::json& j, const WelcomeRespDTO& msg) {
}

inline void from_json(const nlohmann::json& j, WelcomeRespDTO& msg) {
}