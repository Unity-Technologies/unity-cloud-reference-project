using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Unity.ReferenceProject.Tests.Editor
{
    public class EmptyConsoleTests
    {
        readonly List<string> m_WhiteListedLogs = new()
        {
            "Executing IPrebuildSetup",
            "##utp:",
            "App ID successfully set",
            "Running tests for editmode",
            "With test categories:",
            "With test settings file:",
            "Executing tests with settings:",
            "[Code Coverage]"
        };
        LogEntriesWrapper m_LogEntriesWrapper;

        [OneTimeSetUp]
        public void Initialize()
        {
            m_LogEntriesWrapper = new LogEntriesWrapper();
        }

        // Make sure it is run before anything else that can affect the console
        [Test, Order(1)]
        public void TestCleanConsole()
        {
            var msg = $"This message makes sure our test is reading Logs properly {DateTime.Now}";

            Debug.Log(msg);

            m_LogEntriesWrapper.StartGettingEntries();

            var count = m_LogEntriesWrapper.GetCount();

            var log = m_LogEntriesWrapper.GetEntryMessage(count - 1);

            Assert.IsTrue(log.StartsWith(msg));

            if (count == 1)
            {
                Assert.Pass();
                return;
            }

            var ss = new StringBuilder();

            for (var i = 0; i < count - 1; ++i)
            {
                log = m_LogEntriesWrapper.GetEntryMessage(i);

                if (m_WhiteListedLogs.Any(key => log.TrimStart(' ', '\r', '\n').StartsWith(key)))
                    continue;

                ss.AppendLine(log);
            }

            m_LogEntriesWrapper.EndGettingEntries();

            var logs = ss.ToString();

            Assert.That(logs, Is.Empty, $"Found logs in the console on project startup.\nLogs -------\n{logs}\n------------");
        }
    }

    // UnityEditor.LogEntries and UnityEditor.LogEntry are internal. Use Reflection to access them.
    class LogEntriesWrapper
    {
        readonly MethodInfo m_EndGettingEntries;
        readonly MethodInfo m_GetCount;
        readonly MethodInfo m_GetLogEntryInfo;
        readonly FieldInfo m_LogEntryMessage;

        readonly Type m_LogEntryType;
        readonly MethodInfo m_StartGettingEntries;

        public LogEntriesWrapper()
        {
            var logEntriesType = Assembly.GetAssembly(typeof(SceneView)).GetType("UnityEditor.LogEntries");
            Assert.NotNull(logEntriesType);

            m_LogEntryType = Assembly.GetAssembly(typeof(SceneView)).GetType("UnityEditor.LogEntry");
            Assert.NotNull(m_LogEntryType);

            m_GetCount = logEntriesType.GetMethod("GetCount", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(m_GetCount);

            m_StartGettingEntries = logEntriesType.GetMethod("StartGettingEntries", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(m_StartGettingEntries);

            m_GetLogEntryInfo = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(m_GetLogEntryInfo);

            m_EndGettingEntries = logEntriesType.GetMethod("EndGettingEntries", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(m_EndGettingEntries);

            m_LogEntryMessage = m_LogEntryType.GetField("message", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(m_LogEntryMessage);
        }

        public int GetCount()
        {
            return (int)m_GetCount.Invoke(null, null);
        }

        public void StartGettingEntries()
        {
            m_StartGettingEntries.Invoke(null, null);
        }

        public string GetEntryMessage(int index)
        {
            var entry = Activator.CreateInstance(m_LogEntryType);
            m_GetLogEntryInfo.Invoke(null, new[] { index, entry });

            var message = m_LogEntryMessage.GetValue(entry) as string;

            return message;
        }

        public void EndGettingEntries()
        {
            m_EndGettingEntries.Invoke(null, null);
        }
    }
}
