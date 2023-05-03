using System;

namespace UnityEngine.Dt.App.Core
{
    /// <summary>
    /// Handler gives you the ability to send and receive <see cref="Message"/> objects.
    /// <para></para>
    /// When you create a new <see cref="Handler"/> it is bound to a <see cref="Looper"/>.
    /// It will deliver messages to that Looper's message queue and execute them.
    /// </summary>
    public class Handler
    {
        readonly Func<Message, bool> m_Callback;

        readonly Looper m_Looper;

        /// <summary>
        /// Construct an <see cref="Handler"/> instance using a provided <see cref="Looper"/> and
        /// a callback to handle received messages..
        /// </summary>
        /// <param name="looper">The looper.</param>
        /// <param name="callback">The callback to handle received messages.</param>
        /// <exception cref="ArgumentNullException">Some arguments are null.</exception>
        public Handler(Looper looper, Func<Message, bool> callback)
        {
            m_Looper = looper ?? throw new ArgumentNullException(nameof(looper));
            m_Callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <summary>
        /// Remove any pending posts of callbacks and sent messages whose obj is token.
        /// If token is null, all callbacks and messages will be removed.
        /// </summary>
        /// <param name="token">An object (it can be null).</param>
        public void RemoveCallbacksAndMessages(object token)
        {
            m_Looper.RemoveCallbacksAndMessages(token);
        }

        /// <summary>
        /// Send a message that will be enqueued into the <see cref="Looper"/>'s message queue later.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        /// <param name="durationMs">The enqueue delay, in milliseconds.</param>
        public void SendMessageDelayed(Message msg, int durationMs)
        {
            m_Looper.EnqueueMessage(msg, durationMs);
        }

        /// <summary>
        /// Send a message to the <see cref="Looper"/> message queue.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        public void SendMessage(Message msg)
        {
            SendMessageDelayed(msg, 0);
        }

        /// <summary>
        /// Handle a <see cref="Message"/> that just has been dequeued.
        /// <remarks>By default the <see cref="Handler"/> callback will be invoked here.</remarks>
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal bool HandleMessage(Message message)
        {
            return m_Callback.Invoke(message);
        }

        /// <summary>
        /// Get a <see cref="Message"/> from the message pool.
        /// </summary>
        /// <param name="id">The message ID</param>
        /// <param name="obj">An arbitrary object that will be passed with the message.</param>
        /// <returns>A <see cref="Message"/> instance.</returns>
        public Message ObtainMessage(int id, object obj)
        {
            return ObtainMessage(id, 0, obj);
        }

        /// <summary>
        /// Get a <see cref="Message"/> from the message pool.
        /// </summary>
        /// <param name="id">The message ID</param>
        /// <param name="arg1">An arbitrary integer value that will be passed with the message.</param>
        /// <param name="obj">An arbitrary object that will be passed with the message.</param>
        /// <returns>A <see cref="Message"/> instance.</returns>
        public Message ObtainMessage(int id, int arg1, object obj)
        {
            return Message.Obtain(this, id, arg1, obj);
        }
    }
}
