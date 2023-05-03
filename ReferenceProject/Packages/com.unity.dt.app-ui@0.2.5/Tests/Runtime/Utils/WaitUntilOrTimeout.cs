using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace UnityEngine.Dt.App.Tests
{
    /// <summary>
    /// A custom yield instruction that waits until a predicate is true or a timeout is reached.
    /// </summary>
    public class WaitUntilOrTimeOut : CustomYieldInstruction
    {
        static readonly TimeSpan k_DefaultTimeout = TimeSpan.FromSeconds(5);

        readonly Func<bool> m_Predicate;
        readonly Stopwatch m_Stopwatch;
        readonly TimeSpan m_Timeout;
        readonly bool m_FailTestOnTimeout;
        readonly string m_CallerMemberName;
        readonly string m_CallerFilePath;
        readonly int m_CallerLineNumber;

        /// <summary>
        /// Returns true if the instruction should keep waiting.
        /// </summary>
        public override bool keepWaiting
        {
            get
            {
                if (m_Predicate())
                    return false;  // Condition reached: stop waiting.

                if (m_Stopwatch.Elapsed < m_Timeout)
                    return true;   // Keep waiting.

                if (m_FailTestOnTimeout) // Timeout: stop waiting and fail test.
                    Assert.Fail($"Timeout after {m_Stopwatch.ElapsedMilliseconds} ms\n" +
                        $"  at {m_CallerMemberName}() in {m_CallerFilePath}:{m_CallerLineNumber}");

                // Timeout: stop waiting and quit gracefully.
                return false;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="predicate"> The predicate to wait for. </param>
        /// <param name="failTestOnTimeout"> If true, the test will fail if the timeout is reached. </param>
        /// <param name="timeout"> The timeout. </param>
        /// <param name="callerMemberName"> The name of the calling method. </param>
        /// <param name="callerFilePath"> The path of the calling file. </param>
        /// <param name="callerLineNumber"> The line number of the calling method. </param>
        public WaitUntilOrTimeOut(Func<bool> predicate,
                                  bool failTestOnTimeout = true,
                                  TimeSpan? timeout = null,
                                  [CallerMemberName] string callerMemberName = "",
                                  [CallerFilePath] string callerFilePath = "",
                                  [CallerLineNumber] int callerLineNumber = 0)
        {
            m_Predicate = predicate;
            m_Stopwatch = Stopwatch.StartNew();
            m_FailTestOnTimeout = failTestOnTimeout;
            m_Timeout = timeout ?? k_DefaultTimeout;
            m_CallerMemberName = callerMemberName;
            m_CallerFilePath = callerFilePath;
            m_CallerLineNumber = callerLineNumber;
        }
    }
}
