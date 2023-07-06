using System.Collections.Generic;
using Unity.Cloud.Common;
using Unity.ReferenceProject.UITableListView;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.ScenesList
{
    public class ColumnSceneListSceneId : TableListColumn
    {
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

            button.userData = dataSet.Id.ToString();
            button.title = dataSet.Id.ToString();
        }

        string GetButtonName(IColumnData columnData) => $"{nameof(ColumnSceneListSceneId)}-{columnData.Name}";
        
        void OnButtonClick(Button button)
        {
            if(button.userData is not string sceneId)
                return;

            GUIUtility.systemCopyBuffer = sceneId;
            Debug.Log($"Copied to clipboard: {sceneId}");
        }
    }
}
