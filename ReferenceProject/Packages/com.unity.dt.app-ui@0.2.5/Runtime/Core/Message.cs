using System;

namespace UnityEngine.Dt.App.Core
{
    /// <summary>
    /// An object passed in a message queue.
    /// </summary>
    public class Message
    {
        const int k_MaxPoolSize = 50;

        static readonly object k_PoolSync = new object();

        static Message s_Pool;

        static int s_PoolSize;

        Message m_Next;

        /// <summary>
        /// The message ID.
        /// </summary>
        public int what { get; private set; }

        /// <summary>
        /// An arbitrary object to attach with the message.
        /// </summary>
        public object obj { get; private set; }

        /// <summary>
        /// An arbitrary integer value to attach with the message.
        /// </summary>
        public int arg1 { get; private set; }

        /// <summary>
        /// The <see cref="Handler"/> instance which has obtained this message.
        /// </summary>
        public Handler target { get; private set; }

        internal DateTime scheduledTime { get; set; }

        /// <summary>
        /// Recycle this message and put if back into the pool.
        /// </summary>
        public void Recycle()
        {
            //todo check if it is safe to recycle the message
            RecycleUnchecked();
        }

        void RecycleUnchecked()
        {
            what = 0;
            obj = null;
            arg1 = 0;
            target = null;

            lock (k_PoolSync)
            {
                if (s_PoolSize < k_MaxPoolSize)
                {
                    m_Next = s_Pool;
                    AddMessageToPool(this);
                }
            }
        }

        static void AddMessageToPool(Message message)
        {
            s_Pool = message;
            s_PoolSize++;
        }

        /// <summary>
        /// Obtain a message from the pool.
        /// </summary>
        /// <returns>The message.</returns>
        static Message Obtain()
        {
            lock (k_PoolSync)
            {
                if (s_Pool != null)
                {
                    var msg = s_Pool;
                    s_Pool = msg.m_Next;
                    msg.m_Next = null;
                    return msg;
                }
            }

            return new Message();
        }

        /// <summary>
        /// Obtain a message from the pool with pre specified properties.
        /// </summary>
        /// <param name="handler">The target.</param>
        /// <param name="id">The message ID.</param>
        /// <param name="obj">An arbitrary object.</param>
        /// <returns>The message.</returns>
        public static Message Obtain(Handler handler, int id, object obj)
        {
            var msg = Obtain();
            msg.target = handler;
            msg.what = id;
            msg.obj = obj;
            return msg;
        }

        /// <summary>
        /// Obtain a message from the pool with pre specified properties.
        /// </summary>
        /// <param name="handler">The target.</param>
        /// <param name="id">The message ID.</param>
        /// <param name="arg1">An arbitrary integer value.</param>
        /// <param name="obj">An arbitrary object.</param>
        /// <returns>The message.</returns>
        public static Message Obtain(Handler handler, int id, int arg1, object obj)
        {
            var msg = Obtain();
            msg.target = handler;
            msg.what = id;
            msg.arg1 = arg1;
            msg.obj = obj;
            return msg;
        }
    }
}
