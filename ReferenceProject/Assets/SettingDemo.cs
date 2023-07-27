using System.Collections;
using System.Collections.Generic;
using Unity.ReferenceProject.Settings;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public class SettingDemo : MonoBehaviour
    {
        [Inject]
        void Setup(IGlobalSettings globalSettings)
        {
            globalSettings.AddSetting(new ActionSetting("Test", () => Debug.Log("Test")));
        }
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
