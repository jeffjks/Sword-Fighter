using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ThreadManager : MonoBehaviour
{
    private static readonly ConcurrentQueue<UnityAction> _executeOnMainThread = new ConcurrentQueue<UnityAction>();

    private void Update()
    {
        UpdateMain();
    }

    /// <summary>Sets an action to be executed on the main thread.</summary>
    /// <param name="action">The action to be executed on the main thread.</param>
    public static void ExecuteOnMainThread(UnityAction action)
    {
        if (action == null)
        {
            Debug.Log("No action to execute on main thread!");
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