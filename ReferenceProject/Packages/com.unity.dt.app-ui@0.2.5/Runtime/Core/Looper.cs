using System;
using System.Collections.Concurrent;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.Core
{
    /// <summary>
    /// An object that contains its own message queue and its own running loop for dequeuing messages to pass
    /// them to <see cref="Handler"/> targets.
    /// </summary>
    public class Looper
    {
        const int k_MaxMessagesPerFrame = 100;

        readonly IVisualElementScheduler m_Scheduler;

        bool m_Exiting;

        bool m_InLoop;

        IVisualElementScheduledItem m_LoopItem;

        readonly ConcurrentQueue<Message> m_PendingMessages;

        static Looper s_MainLooper;

        /// <summary>
        /// Default constructor using the Unity UI-Toolkit VisualElement scheduler to run the loop.
        /// </summary>
        /// <param name="scheduler">The Unity UI-Toolkit VisualElement scheduler which will execute the loop.</param>
        public Looper(IVisualElementScheduler scheduler)
            : this()
        {
            m_Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Looper()
        {
            queue = new ConcurrentQueue<Message>();
            m_PendingMessages = new ConcurrentQueue<Message>();
        }

        /// <summary>
        /// The looper's message queue.
        /// </summary>
        public ConcurrentQueue<Message> queue { get; }

        /// <summary>
        /// Check if the Looper is currently active.
        /// </summary>
        public bool inLoop => m_InLoop;

        /// <summary>
        /// Start the loop execution.
        /// </summary>
        public void Loop()
        {
            if (m_InLoop)
            {
                Debug.LogWarning("Loop called on a Looper already in a loop");
                return;
            }

            m_InLoop = true;
            
            // scheduler is only used in testing environment
            m_LoopItem = m_Scheduler?.Execute(LoopOnce).Every(16L);
        }

        internal void LoopOnce()
        {
            EnqueuePendingMessages();

            if (!m_InLoop)
                throw new InvalidOperationException("LoopOnce should never be called when not in a Loop");

            if (m_Exiting)
                return;

            var dequeuedMessages = 0;
            while (dequeuedMessages < k_MaxMessagesPerFrame && queue.TryDequeue(out var msg))
            {
                if (msg.target.HandleMessage(msg))
                {
                    msg.Recycle();
                }
                else
                {
                    // not handled yet, put the message in the queue again
                    queue.Enqueue(msg);
                }

                dequeuedMessages++;
            }
        }

        /// <summary>
        /// Promptly stop the loop execution and clear the remaining message without handling them.
        /// <remarks>If you want to safely stop the execution of the loop and handle remaining messages in the queue, use <see cref="SafelyQuit"/>.</remarks>
        /// </summary>
        public void Quit()
        {
            m_LoopItem?.Pause();
            m_LoopItem = null;
            queue.Clear();
            m_InLoop = false;
            m_Exiting = true;
        }

        /// <summary>
        /// Stop the loop execution but handle all remaining messages in the queue before clearing it.
        /// </summary>
        public void SafelyQuit()
        {
            m_LoopItem?.Pause();
            m_LoopItem = null;
            var messages = queue.ToArray();
            queue.Clear();
            foreach (var msg in messages)
            {
                msg.target.HandleMessage(msg);
                msg.Recycle();
            }

            m_InLoop = false;
            m_Exiting = true;
        }

        internal void EnqueueMessage(Message msg, int delayMs)
        {
            if (msg == null)
                throw new ArgumentNullException(nameof(msg));

            msg.scheduledTime = DateTime.Now.AddMilliseconds(delayMs);

            if (delayMs <= 0)
            {
                queue.Enqueue(msg);
                return;
            }

            m_PendingMessages.Enqueue(msg);
        }

        internal void RemoveCallbacksAndMessages(object token)
        {
            if (token == null)
            {
                m_PendingMessages.Clear();
                queue.Clear();
            }
            else
            {
                if (m_PendingMessages.Count > 0)
                {
                    var delayedMessages = m_PendingMessages.ToArray();
                    m_PendingMessages.Clear();
                    foreach (var delayedMessage in delayedMessages)
                    {
                        if (delayedMessage.obj != token)
                            m_PendingMessages.Enqueue(delayedMessage);
                    }
                }

                if (queue.Count > 0)
                {
                    var messages = queue.ToArray();
                    queue.Clear();
                    foreach (var message in messages)
                    {
                        if (message.obj != token)
                            queue.Enqueue(message);
                    }
                }
            }
        }

        void EnqueuePendingMessages()
        {
            if (m_PendingMessages.Count == 0)
                return;

            var now = DateTime.Now;
            var delayedMessages = m_PendingMessages.ToArray();
            m_PendingMessages.Clear();
            foreach (var msg in delayedMessages)
            {
                if (msg.scheduledTime > now)
                    m_PendingMessages.Enqueue(msg);
                else
                    queue.Enqueue(msg);
            }
        }
    }
}
