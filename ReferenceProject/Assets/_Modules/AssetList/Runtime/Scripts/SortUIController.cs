using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.AssetList
{
    public class SortUIController : MonoBehaviour
    {
        [SerializeField]
        Dropdown m_SortDropdown;

        [SerializeField]
        Button m_OrderButton;

        readonly string k_SortDropdown = "SortDropdown";
        readonly string k_SortOrder = "SortOrder";

        public void InitUIToolkit(VisualElement root)
        {
            m_SortDropdown = root.Q<Dropdown>(k_SortDropdown);
            m_OrderButton = root.Q<Button>(k_SortOrder);

            // Hide feature until it is implemented
            var sortContainer = root.Q("SortOptions");
            sortContainer.style.display = DisplayStyle.None;
        }
    }
}
