using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SwordFighterServer
{
    class ThreadManager
    {
        private static readonly ConcurrentQueue<Action> _executeOnMainThread = new ConcurrentQueue<Action>();

        /// <summary>Sets an action to be executed on the main thread.</summary>
        /// <param name="action">The action to be executed on the main thread.</param>
        public static void ExecuteOnMainThread(Action action)
        {
            if (action == null)
            {
                Console.WriteLine("No action to execute on main thread!");
                return;
            }

            _executeOnMainThread.Enqueue(action);
        }

        /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
        public static void UpdateMain()
        {
            while (_executeOnMainThread.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }
    }
}
