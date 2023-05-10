using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.ReferenceProject.Instructions
{
    public class InstructionUIEntryTouchScreen : InstructionUIEntry
    {
#if UNITY_IOS || UNITY_ANDROID
        protected override bool IsSupportPlatform => true;
#else
        protected override bool IsSupportPlatform => false;
#endif
    }
}
