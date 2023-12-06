using Unity.ReferenceProject.AssetList;
using Unity.ReferenceProject.CustomKeyboard;
using UnityEngine;

namespace Unity.ReferenceProject.VR
{
    public class AssetListKeyboardAdapter : MonoBehaviour
    {
        [SerializeField]
        AssetListUIController m_AssetListUIController;

        [SerializeField]
        KeyboardHandler m_KeyboardHandler;

        // Start is called before the first frame update
        void Start()
        {
            m_KeyboardHandler.RegisterRootVisualElement(m_AssetListUIController.RootVisualElement);
        }
    }
}
