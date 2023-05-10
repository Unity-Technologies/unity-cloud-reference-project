using System;
using UnityEngine;

namespace Unity.ReferenceProject
{
    public class SwitchFlag : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
