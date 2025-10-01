using System;
using System.Collections.Generic;
using System.Text;

namespace SwordFighterServer
{
    class GameLogic
    {
        private static int broadcastTimer;
        private readonly static int msPerTick = 1000 / Constants.TICKS_PER_SEC;

        public static int CurrentTick { get; private set; } = 0;

        private const int BroadcastPeriodMs = 200;

        public static void Update()
        {
            broadcastTimer += msPerTick;
            CurrentTick++;

            bool isBroadcasting = broadcastTimer >= BroadcastPeriodMs;

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
            {
                // Console.WriteLine($"[Broadcast] CurrentTick: {CurrentTick}, ElapsedTime: {Server.ElapsedMs}, TickToTimestamp(CurrentTick): {Server.TickToTimestamp(CurrentTick)}");
                broadcastTimer -= BroadcastPeriodMs;
            }

            ThreadManager.UpdateMain();
        }
    }
}
