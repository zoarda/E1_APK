using System;
using System.Collections.Generic;
using UnityEngine;

namespace Games.Coresdk.Unity
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        Queue<Action> m_queue;

        private void Awake()
        {
            m_queue = new Queue<Action>();
        }

        private void Update()
        {
            while (m_queue.Count > 0)
            {
                m_queue.Dequeue().Invoke();
            }
        }

        public void Add(Action action)
        {
            m_queue.Enqueue(action);
        }
    }
}
