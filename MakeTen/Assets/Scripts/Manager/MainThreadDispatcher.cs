using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : Singleton<MainThreadDispatcher>
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    protected override void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                try
                {
                    _executionQueue.Dequeue()?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }

    public void Enqueue(Action action)
    {
        if (action == null) return;

        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    public static void EnqueueToMainThread(Action action)
    {
        Instance.Enqueue(action);
    }
}
