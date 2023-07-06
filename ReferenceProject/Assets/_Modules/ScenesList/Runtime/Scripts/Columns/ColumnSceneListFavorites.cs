using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Cloud.Common;
using Unity.ReferenceProject.UITableListView;
using Unity.ReferenceProject.SearchSortFilter;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Clickable = Unity.AppUI.UI.Clickable;

namespace Unity.ReferenceProject.ScenesList
{
    public class ColumnSceneListFavorites : TableListColumn
    {
        [SerializeField]
        string m_IconName;
        
        [SerializeField]
        string m_ButtonName;

        [SerializeField]
        string m_DisabledStyle;
        
        readonly HashSet<string> m_MarkedSet = new ();
        
        Saves m_Saves;

        class Saves
        {
            public List<string> IdList = new List<string>();
            
            readonly string m_SavePath;

            public Saves(string savePath)
            {
                m_SavePath = savePath;
                Load(m_SavePath);
            }

            public void Save()
            {
                string data = JsonUtility.ToJson(this);
                File.WriteAllText(m_SavePath, data);
            }
            
            public void Load(string loadPath)
            {
                if (File.Exists(loadPath))
                {
                   var saves = JsonUtility.FromJson<Saves>(File.ReadAllText(loadPath));
                   IdList.Clear();
                   IdList.AddRange(saves.IdList);
                }
            }
        }

        void Awake()
        {
            m_Saves = new Saves(Application.persistentDataPath + "/SceneListFavorites.json");
            
            foreach (var scene in m_Saves.IdList)
            {
                m_MarkedSet.Add(scene);
            }
        }
        
        protected override void AddServices(List<object> services)
        {
            base.AddServices(services);
            services.Add(new SortBindNodeInt<object>(x =>
                x is IScene dataSet && m_MarkedSet.Contains(dataSet.Id.ToString()) ? -1 : 1));
        }

        protected override void OnCreateHeader(VisualElement e, IColumnData columnData)
        {
            TableListColumnData.BuildIconHeader(e, columnData);
        }
        
        protected override void OnMakeCell(VisualElement e, IColumnData columnData)
        {
            var button = new IconButton
            {
                name = GetButtonName(columnData.Name),
                primary = false,
                quiet = true,
                focusable = false
            };

            button.AddManipulator(new Clickable((_) => OnButtonClick(button)));
            
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
            
            var button = e.Q<IconButton>(GetButtonName(columnData.Name));

            if (button == null) 
                return;

            var sceneId = dataSet.Id.ToString();
            RefreshIcon(button, m_MarkedSet.Contains(sceneId));
            button.userData = sceneId;
        }
        
        protected override void OnPointerEnterListElementEvent(PointerEnterEvent pointerEventData)
        {
            if (pointerEventData.target is not VisualElement hovered) 
                return;
            
            var button = hovered.Q<IconButton>(GetButtonName(ColumnName));
            
            if (button == null) 
                return;
            
            button.quiet = false;
            button.icon = m_IconName;

            if (button.userData is string sceneId && m_MarkedSet.Contains(sceneId))
            {
                button.AddToClassList(m_DisabledStyle);
            }
        }

        protected override void OnPointerLeaveListElementEvent(PointerLeaveEvent pointerEventData)
        {
            if (pointerEventData.target is not VisualElement hovered) 
                return;
            
            var button = hovered.Q<IconButton>(GetButtonName(ColumnName));
            
            if (button == null) 
                return;
            
            button.quiet = true;
            
            if (button.userData is string sceneId && !m_MarkedSet.Contains(sceneId))
            {
                button.icon = null;
            }
            button.RemoveFromClassList(m_DisabledStyle);
        }

        string GetButtonName(string columnName) => $"{m_ButtonName}-{columnName}";

        void OnButtonClick(IconButton button)
        {
            if(button.userData is not string sceneId)
                return;
            
            if (m_MarkedSet.Contains(sceneId))
            {
                button.AddToClassList(m_DisabledStyle);
                m_MarkedSet.Remove(sceneId);
            }
            else
            {
                RefreshIcon(button, true);
                button.RemoveFromClassList(m_DisabledStyle);
                m_MarkedSet.Add(sceneId);
            }

            m_Saves.IdList = m_MarkedSet.ToList();
            m_Saves.Save();
        }

        void RefreshIcon(IconButton button, bool isOn) => button.icon = isOn ? m_IconName : null;
    }
}
