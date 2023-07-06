using System;
using System.Linq;
using Unity.ReferenceProject.Presence;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.AccessHistory
{
    public class AccessHistoryUIController : MonoBehaviour
    {
        [SerializeField]
        UIDocument UIDocument;

        [SerializeField]
        VisualTreeAsset AccessElementTemplate;

        IAccessHistoryController m_AccessHistoryController;
        public static string AccessHistoryVEID { get; } = "access-history";

        void Setup(IAccessHistoryController accessHistoryController)
        {
            m_AccessHistoryController = accessHistoryController;
        }

        void RefreshAccessHistoryContainer()
        {
            var container = UIDocument.rootVisualElement.Q<VisualElement>(AccessHistoryVEID);

            container.Clear();

            foreach (var id in m_AccessHistoryController.GetCount(4).Select(x => x.id))
            {
                var accessElementTemplate = AccessElementTemplate.Instantiate();

                accessElementTemplate.Q<Heading>("project-name").text = id.ToString();
                accessElementTemplate.Q<Heading>("workspace-name").text = id.ToString();

                container.Add(accessElementTemplate);
            }
        }
    }
}
