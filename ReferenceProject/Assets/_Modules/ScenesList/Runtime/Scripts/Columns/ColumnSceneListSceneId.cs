using System.Collections.Generic;
using Unity.Cloud.Common;
using Unity.ReferenceProject.UITableListView;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.Dt.App.UI.Button;

namespace Unity.ReferenceProject.ScenesList
{
    public class ColumnSceneListSceneId : TableListColumn
    {
        readonly Dictionary<Button, string> m_ButtonMap = new();

        protected override void OnCreateHeader(VisualElement e, IColumnData columnData)
        {
            TableListColumnData.BuildTextHeader(e, columnData);
        }
        
        protected override void OnMakeCell(VisualElement e, IColumnData columnData)
        {
            var button = new Button();
            button.name = GetButtonName(columnData);
            button.style.flexGrow = 1;
            
            button.clickable.clicked += () => OnButtonClick(button);
            e.Add(button);
        }
        
        protected override void OnBindCell(VisualElement e, IColumnData columnData, object data)
        {
            if (data is not IScene dataSet)
            {
                Debug.LogWarning(
                    $"Wrong type of data pasted to the column `{columnData.Name}`. Expected type is {nameof(IScene)}");
                return;
            }

            var button = e.Q<Button>(GetButtonName(columnData));
            
            m_ButtonMap[button] = dataSet.Id.ToString();
            button.title = dataSet.Id.ToString();
        }

        protected override void OnReset()
        {
            base.OnReset();
            m_ButtonMap.Clear();
        }

        string GetButtonName(IColumnData columnData) => $"{nameof(ColumnSceneListSceneId)}-{columnData.Name}";
        
        void OnButtonClick(Button button)
        {
            if (m_ButtonMap.TryGetValue(button, out var value))
            {
                GUIUtility.systemCopyBuffer = value;
                Debug.Log($"Copied to clipboard: {value}");
            }
        }
    }
}
