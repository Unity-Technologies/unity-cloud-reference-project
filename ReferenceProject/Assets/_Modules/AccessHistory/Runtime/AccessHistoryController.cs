using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Cloud.Common;
using UnityEngine;

namespace Unity.ReferenceProject.AccessHistory
{
    public interface IAccessHistoryController
    {
        public bool Accessed(SceneId id, out DateTime time);
        public IReadOnlyList<(SceneId id, DateTime accessed)> GetCount(int count);
        public ReadOnlyDictionary<SceneId, DateTime> GetHistory();
        public void AddData(IScene scene);
    }

    public class AccessHistoryController : IAccessHistoryController
    {
        const string LastAccessFileName = "LastAccess.json";
        readonly IDictionary<SceneId, DateTime> m_accessHistory;
        readonly string m_LastAccessPath;

        public AccessHistoryController()
        {
            m_LastAccessPath = FormLastAccessPathName();
            m_accessHistory = ReadAccessHistoryOrCreateEmpty();
        }

        public bool Accessed(SceneId id, out DateTime time) => m_accessHistory.TryGetValue(id, out time);

        public IReadOnlyList<(SceneId id, DateTime accessed)> GetCount(int count)
        {
            return m_accessHistory.OrderBy(x => x.Value)
                .Take(count)
                .Select(x => (x.Key, x.Value))
                .ToList();
        }

        public void AddData(IScene scene)
        {
            UpdateHistory(scene);
        }

        public ReadOnlyDictionary<SceneId, DateTime> GetHistory()
        {
            return new(m_accessHistory);
        }

        static string FormLastAccessPathName()
        {
            var sb = new StringBuilder();
            sb.Append(Application.persistentDataPath);
            sb.Append(Path.DirectorySeparatorChar);
            sb.Append($"{LastAccessFileName}");
            return sb.ToString();
        }

        IDictionary<SceneId, DateTime> ReadAccessHistoryOrCreateEmpty()
        {
            // There has to be a trimmer way, but SceneId won't parse from a string
            if (File.Exists(m_LastAccessPath))
            {
                var json = File.ReadAllText(m_LastAccessPath);
                var data = JsonSerialization.Deserialize<Dictionary<string, DateTime>>(json);
                var toReturn = new Dictionary<SceneId, DateTime>();

                foreach (var kvp in data)
                {
                    toReturn[new SceneId(kvp.Key)] = kvp.Value;
                }

                return toReturn;
            }

            return new Dictionary<SceneId, DateTime>();
        }

        void UpdateHistory(IScene scene)
        {
            m_accessHistory[scene.Id] = DateTime.Now;
            WriteAccessHistory();
        }

        void WriteAccessHistory()
        {
            var data = JsonSerialization.Serialize(m_accessHistory);
            File.WriteAllText(m_LastAccessPath, data);
        }
    }
}
