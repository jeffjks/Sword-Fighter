//common.h
#pragma once
#pragma comment(lib,"ws2_32")
#pragma warning(disable:4996)

const int MAX_PLAYERS = 4;
const int PORT = 26960;
const int SERVER_MESSAGE = 127;
const int ADMIN_MESSAGE = 126;
const int MS_BROADCASTING = 100; // 채팅 전송 주기
const string VERSION = "1.0";