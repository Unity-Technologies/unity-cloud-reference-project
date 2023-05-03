using System;
using UnityEngine;

namespace Unity.ReferenceProject.Instructions
{
    public class InstructionUIEntryPC : InstructionUIEntry
    {
        protected override bool IsSupportPlatform => Application.platform is RuntimePlatform.WindowsPlayer or RuntimePlatform.WindowsEditor;
    }
}
