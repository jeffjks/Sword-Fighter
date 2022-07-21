//common.h
#pragma once
#include <queue>
#include <mutex>
#include <condition_variable>
#include <iostream>
#include "packet.h"
#pragma comment(lib,"ws2_32")
#pragma warning(disable:4996)

static queue<pair<int, string>> messageQueue;
//mutex mtx;