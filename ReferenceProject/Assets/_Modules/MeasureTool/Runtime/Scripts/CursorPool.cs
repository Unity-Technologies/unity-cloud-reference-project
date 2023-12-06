using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool
{
    public interface ICursorPool
    {
        GameObject GetAvailableCursor();
        void ResetCursors();
        void SetColor(Color color);
        bool IsCursor(GameObject gameObject);
    }
    
    
    [Serializable]
    public class CursorPool : ICursorPool
    {
        [SerializeField]
        GameObject[] m_Cursors;
        
        List<GameObject> m_AvailableCursors;
        List<GameObject> m_UsedCursors;
        readonly List<Tuple<GameObject, MeshRenderer>> m_CachedRenderers = new();

        public void Initialize()
        {
            if (m_AvailableCursors != null)
                return;
            
            m_AvailableCursors = new List<GameObject>(m_Cursors);
            m_UsedCursors = new List<GameObject>(m_AvailableCursors.Count);

            foreach (var cursor in m_AvailableCursors)
            {
                foreach (var mesh in cursor.GetComponentsInChildren<MeshRenderer>())
                    m_CachedRenderers.Add(new Tuple<GameObject, MeshRenderer>(cursor, mesh));
            }
        }

        public GameObject GetAvailableCursor()
        {
            var cursor = m_AvailableCursors[0];
            m_AvailableCursors.RemoveAt(0);
            m_UsedCursors.Add(cursor);
            
            return cursor;
        }
        
        public bool IsCursor(GameObject gameObject)
        {
            return m_UsedCursors.Contains(gameObject);
        }

        public void ResetCursor(GameObject cursor)
        {
            if (!m_UsedCursors.Contains(cursor))
                return;

            cursor.SetActive(false);
            m_UsedCursors.Remove(cursor);
            m_AvailableCursors.Add(cursor);
        }

        public void ResetCursors()
        {
            if (m_UsedCursors == null)
                return;
            
            //Return cursors to pool
            for (var i = m_UsedCursors.Count - 1; i >= 0; i--)
            {
                ResetCursor(m_UsedCursors[i]);
            }
        }

        public void SetColor(Color color)
        {
            foreach (var tuple in m_CachedRenderers)
                tuple.Item2.material.color = color;
        }
    }
}