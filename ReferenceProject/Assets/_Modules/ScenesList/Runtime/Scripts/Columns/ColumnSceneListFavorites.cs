using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Cloud.Common;
using Unity.ReferenceProject.UITableListView;
using Unity.ReferenceProject.SearchSortFilter;
using UnityEngine;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;

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
        
        readonly HashSet<string> m_HashSet = new ();
        
        readonly Dictionary<VisualElement, string> m_ButtonMap = new();
        
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
                m_HashSet.Add(scene);
            }
        }
        
        protected override void AddServices(List<object> services)
        {
            base.AddServices(services);
            services.Add(new SortBindNodeInt<object>(x =>
                x is IScene dataSet && m_HashSet.Contains(dataSet.Id.ToString()) ? -1 : 1));
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

            button.clickable = null; // AppUI 0.2.9 has currently a bug with Pressables. Use a Clickable manipulator instead.
            var manipulator = new UnityEngine.Dt.App.UI.Clickable(() => OnButtonClick(button));
            button.AddManipulator(manipulator);
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
            
            RefreshIcon(button, m_HashSet.Contains(dataSet.Id.ToString()));
            m_ButtonMap[button] = dataSet.Id.ToString();
        }
        
        protected override void OnMouseEnterListElementEvent(MouseEnterEvent mouseEventData)
        {
            if (mouseEventData.target is not VisualElement hovered) 
                return;
            
            var button = hovered.Q<IconButton>(GetButtonName(ColumnName));
            
            if (button == null) 
                return;
            
            button.quiet = false;
            button.icon = m_IconName;
                    
            if(m_ButtonMap.TryGetValue(button, out var guid) && !m_HashSet.Contains(guid))
                button.AddToClassList(m_DisabledStyle);
        }

        protected override void OnMouseLeaveListElementEvent(MouseLeaveEvent mouseEventData)
        {
            if (mouseEventData.target is not VisualElement hovered) 
                return;
            
            var button = hovered.Q<IconButton>(GetButtonName(ColumnName));
            
            if (button == null) 
                return;
            
            button.quiet = true;
            if(m_ButtonMap.TryGetValue(button, out var guid) && !m_HashSet.Contains(guid))
                button.icon = null;
            button.RemoveFromClassList(m_DisabledStyle);
        }

        protected override void OnReset()
        {
            base.OnReset();
            m_ButtonMap.Clear();
        }
        
        string GetButtonName(string columnName) => $"{m_ButtonName}-{columnName}";

        void OnButtonClick(IconButton button)
        {
            if (m_ButtonMap.TryGetValue(button, out var guid))
            {
                if (m_HashSet.Contains(guid))
                {
                    button.AddToClassList(m_DisabledStyle);
                    m_HashSet.Remove(guid);
                }
                else
                {
                    RefreshIcon(button, true);
                    button.RemoveFromClassList(m_DisabledStyle);
                    m_HashSet.Add(guid);
                }

                m_Saves.IdList = m_HashSet.ToList();
                m_Saves.Save();
            }
        }

        void RefreshIcon(IconButton button, bool isOn) => button.icon = isOn ? m_IconName : null;
    }
}
