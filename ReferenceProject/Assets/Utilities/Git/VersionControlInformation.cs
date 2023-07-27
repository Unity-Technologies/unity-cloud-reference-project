using System;
using UnityEngine;

namespace Unity.Cloud.ReferenceProject.Utils.Git
{
    public class VersionControlInformation : ScriptableObject, IVersionControlInformation
    {
        public static readonly string k_AssetName = "VersionControlInformation";

        [SerializeField]
        string m_GitVersionHash;

        public string CommitHash => m_GitVersionHash;

        public void SetHash(string hash) { m_GitVersionHash = hash; }

        static VersionControlInformation s_Instance;

        public static VersionControlInformation Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = Resources.Load<VersionControlInformation>(k_AssetName);
                    if (s_Instance == null)
                    {
                        s_Instance = CreateInstance<VersionControlInformation>();
                    }
                }
                return s_Instance;
            }
        }
    }
}
