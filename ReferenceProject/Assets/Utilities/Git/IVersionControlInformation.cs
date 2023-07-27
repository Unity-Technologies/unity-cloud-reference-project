using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cloud.ReferenceProject.Utils.Git
{
    interface IVersionControlInformation
    {
        string CommitHash { get; }
        void SetHash(string hash);
    }
}
