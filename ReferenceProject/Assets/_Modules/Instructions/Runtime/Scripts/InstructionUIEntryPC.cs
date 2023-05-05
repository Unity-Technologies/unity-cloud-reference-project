using System;
using UnityEngine;

namespace Unity.ReferenceProject.Instructions
{
    public class InstructionUIEntryPC : InstructionUIEntry
    {
#if UNITY_IOS || UNITY_ANDROID
        protected override bool IsSupportPlatform => false;
#else
        protected override bool IsSupportPlatform => true;
#endif
    }
}
