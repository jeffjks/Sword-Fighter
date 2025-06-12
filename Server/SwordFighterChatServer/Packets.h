#pragma once

enum ChatServerPackets
{
    none = 0,
    welcome,
    chatMessage = 101,
    joinChatRoom,
    leaveChatRoom
};