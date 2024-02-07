using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Stats
{
    public interface IGlobalStats
    {
        VisualElement CreateVisualTree();
        void AddStat(IStat stat, uint order = 0);
        void RemoveStat(IStat stat);
        void SetDisplayed(bool isDisplayed);
    }

    public interface IStat
    {
        VisualElement CreateVisualTree();
        void OnStatDisplayed();
        void OnStatHidden();
    }

    public static class GlobalStatsStyleClasses
    {
        public static readonly string EntryStyle = "container__global-stat-entry";
    }

    [Serializable]
    public class GlobalStats : IGlobalStats
    {
        VisualElement m_RootVisualElement;
        Dictionary<IStat, VisualElement> m_Stats = new();
        
        bool m_IsDisplayed;

        public void AddStat(IStat stat, uint order = 0)
        {
            if (stat == null)
                return;

            m_Stats.Add(stat, null);

            if (m_RootVisualElement != null)
            {
                AddStatToVisualTree(stat);

                if (m_IsDisplayed)
                {
                    stat.OnStatDisplayed();
                }
            }
        }

        public void RemoveStat(IStat stat)
        {
            if (stat == null)
                return;

            if (m_RootVisualElement != null)
            {
                RemoveStatFromVisualTree(stat);
            }

            m_Stats.Remove(stat);
        }
        
        public void SetDisplayed(bool isDisplayed)
        {
            if (m_IsDisplayed == isDisplayed)
                return;

            m_IsDisplayed = isDisplayed;

            if (m_IsDisplayed)
            {
                foreach (var stat in m_Stats.Keys)
                {
                    stat.OnStatDisplayed();
                }
            }
            else
            {
                foreach (var stat in m_Stats.Keys)
                {
                    stat.OnStatHidden();
                }
            }
        }

        public VisualElement CreateVisualTree()
        {
            if (m_RootVisualElement != null)
                return m_RootVisualElement;

            m_RootVisualElement = new VisualElement();
            
            foreach (var stat in m_Stats.Keys.ToArray())
            {
                AddStatToVisualTree(stat);
            }

            return m_RootVisualElement;
        }
        
        void AddStatToVisualTree(IStat stat)
        {
            if (m_Stats[stat] != null)
                return;
            
            var element = stat.CreateVisualTree();
            element.AddToClassList(GlobalStatsStyleClasses.EntryStyle);
            m_RootVisualElement.Add(element);
            
            m_Stats[stat] = element;
        }

        void RemoveStatFromVisualTree(IStat stat)
        {
            if (m_Stats.TryGetValue(stat, out var element))
            {
                m_RootVisualElement.Remove(element);
            }

            m_Stats[stat] = null;
        }
    }
}
