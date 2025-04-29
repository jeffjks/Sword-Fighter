using System;
using System.Collections.Generic;
using System.Text;

namespace SwordFighterServer
{
    class GameLogic
    {
        private const float broadcastRate = 5f;
        private static float broadcastTimer = 0f;
        private static float deltaTime = 1f / Constants.TICKS_PER_SEC;

        public static int CurrentTick { get; private set; } = 0;

        public static void Update()
        {
            broadcastTimer += deltaTime;
            CurrentTick++;

            bool isBroadcasting = broadcastTimer >= 1f / broadcastRate;

            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    client.player.Update();
                    if (isBroadcasting)
                    {
                        client.player.BroadcastPlayer();
                    }
                }
            }

            if (isBroadcasting)
                broadcastTimer -= 1f / broadcastRate;

            ThreadManager.UpdateMain();
        }

        public static int GetTickFromTimestamp(long timestamp)
        {
            long elapsedMs = timestamp - Server.serverStartTime;

            if (elapsedMs <= 0)
                return 0;

            return (int) (elapsedMs / Constants.TICKS_PER_SEC); // 결과는 float (소수 Tick 가능)
        }
    }
}
