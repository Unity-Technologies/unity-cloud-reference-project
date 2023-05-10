using System;
using System.Collections.Generic;
using Unity.Cloud.Common;
using Unity.ReferenceProject.SearchSortFilter;

namespace Unity.ReferenceProject.AccessHistory
{
    public class SortByAccessHistoryBindNode : ISortBindNode<IScene>
    {
        readonly IAccessHistoryController m_AccessHistoryController;

        public SortByAccessHistoryBindNode(IAccessHistoryController accessHistoryController)
        {
            m_AccessHistoryController = accessHistoryController;
        }

        public void PerformSort(List<IScene> scenesList, SortOrder sortOrder)
        {
            var history = m_AccessHistoryController.GetHistory();
            scenesList.Sort((scene1, scene2) =>
            {
                if (!history.TryGetValue(scene1.Id, out var date1))
                {
                    date1 = DateTime.MinValue;
                }

                if (!history.TryGetValue(scene2.Id, out var date2))
                {
                    date2 = DateTime.MinValue;
                }

                /*
                 * Inverted sort order because we want time since order instead actual dateTime size order
                 */
                return ((sortOrder == SortOrder.Ascending) ? -1 : 1) * date1.CompareTo(date2);
            });
        }
    }
}
