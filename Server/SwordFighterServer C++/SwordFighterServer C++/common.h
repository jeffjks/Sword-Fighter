//common.h
#pragma once
#include <queue>
#include <mutex>
#include <condition_variable>
#include <iostream>
#include <winsock2.h>
#include "Packet.h"
#pragma comment(lib,"ws2_32")
#pragma warning(disable:4996)

static queue<pair<int, string>> messageQueue;
static mutex mtx;