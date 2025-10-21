using System;
using System.Threading;

namespace SwordFighterServer
{
    class Program
    {
        private static volatile bool isRunning = false;
        private const int Port_TCP = 26950;
        private const int Port_UDP = 26951;

        static void Main(string[] args)
        {
            Console.Title = "Server";

            Server.Start(4, Port_TCP);
            ServerPing.Start(Port_UDP);

            isRunning = true;
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second."); ;

            int executedTicks = 0;

            while (isRunning)
            {
                long elapsedMs = Server.ElapsedMs;

                while (executedTicks < Server.TargetTick) // 타겟 틱만큼 Update 실행
                {
                    try
                    {
                        GameLogic.Update();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[GameLoop] Update error: {ex}");
                    }
                    executedTicks++;
                }

                // 다음 틱까지 남은 시간만큼 Sleep
                long nextTickMs = ((executedTicks + 1L) * 1000L) / Constants.TICKS_PER_SEC;
                long sleepMs = nextTickMs - Server.ElapsedMs;
                if (sleepMs > 0)
                    Thread.Sleep((int)Math.Min(sleepMs, 2));
                else
                    Thread.Yield();
            }
        }
    }
}
