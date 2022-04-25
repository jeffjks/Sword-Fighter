using System;
using System.Threading;

namespace SwordFighterServer
{
    class Program
    {
        private static bool isRunning = false;

        static void Main(string[] args)
        {
            Console.Title = "Server";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(4, 26950);
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");

            DateTime nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (nextLoop < DateTime.Now) // 스레드를 사용하여 매 초 Constants.TICKS_PER_SEC 회 Update() 실행
                {
                    GameLogic.Update();

                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if (nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
