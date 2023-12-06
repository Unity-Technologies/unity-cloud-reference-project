using System;
using System.Collections.Generic;
using Unity.ReferenceProject.DataStores;
using Unity.ReferenceProject.ObjectSelection;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool
{
    [Serializable]
    public abstract class CursorsController
    {
        [SerializeField]
        CursorPool m_CursorPool;

        IAnchorSelector m_AnchorPicker;

        PropertyValue<GameObject> m_SelectedCursor;
        PropertyValue<IObjectSelectionInfo> m_SelectionInfo;

        protected readonly List<GameObject> m_AnchorIndexToCursorObject = new();

        public void Setup(PropertyValue<GameObject> selectedCursor, IAnchorSelector anchorPicker, PropertyValue<IObjectSelectionInfo> selectionInfo)
        {
            m_AnchorPicker = anchorPicker;
            m_SelectedCursor = selectedCursor;
            m_SelectionInfo = selectionInfo;
        }

        void OnSelectionInfoChanged(IObjectSelectionInfo selectionInfo)
        {
            if (selectionInfo == null || float.IsNaN(selectionInfo.SelectedPosition.x))
                return;

            CreateAnchorAtWorldPosition(selectionInfo.SelectedPosition, selectionInfo.SelectedNormal);
        }

        protected MeasureLineData m_SelectedLine;

        public void Open(MeasureLineData targetData)
        {
            m_SelectedLine = targetData;
            m_CursorPool.Initialize();
            SetColor(targetData.Color);
            m_SelectionInfo.ValueChanged += OnSelectionInfoChanged;

            foreach (var anchor in targetData.Anchors)
            {
                CreateCursorForAnchor(anchor);
            }

            UnselectCurrentCursor();
        }

        public void Close()
        {
            m_CursorPool.ResetCursors();
            m_AnchorIndexToCursorObject.Clear();
            m_SelectedLine = null;
            m_SelectionInfo.ValueChanged -= OnSelectionInfoChanged;
        }

        public void Clear()
        {
            m_SelectedLine?.RemoveAnchors();
            m_CursorPool.ResetCursors();
            m_AnchorIndexToCursorObject.Clear();
        }

        public virtual void CreateAnchorAtWorldPosition(Vector3 worldPosition, Vector3 normal)
        {
            var anchor = m_AnchorPicker.Pick(worldPosition, normal);
            m_SelectedLine.AddAnchor(anchor);

            CreateCursorForAnchor(anchor);
        }

        void CreateCursorForAnchor(IAnchor anchor)
        {
            var cursor = GetAvailableCursor();
            m_AnchorIndexToCursorObject.Add(cursor);
            m_SelectedCursor.SetValue(cursor);
            UpdateCursorGameObject(anchor, cursor);
        }

        public void UnselectCurrentCursor()
        {
            m_SelectedCursor.SetValue((GameObject)null);
        }

        public void UpdateDraggableWorldPosition(Vector3 worldPos, Vector3 normal)
        {
            var cursor = m_SelectedCursor.GetValue();
            var anchorIndex = m_AnchorIndexToCursorObject.IndexOf(cursor);

            SetAnchorAtWorldPosition(anchorIndex, worldPos, normal);
        }

        protected virtual void SetAnchorAtWorldPosition(int index, Vector3 worldPosition, Vector3 normal)
        {
            var anchor = m_AnchorPicker.Pick(worldPosition, normal);
            m_SelectedLine.SetAnchor(index, anchor);

            var cursor = m_AnchorIndexToCursorObject[index];
            UpdateCursorGameObject(anchor, cursor);
        }

        public bool IsCursor(GameObject go)
        {
            return m_CursorPool.IsCursor(go);
        }

        public GameObject GetAvailableCursor()
        {
            return m_CursorPool.GetAvailableCursor();
        }

        public void SetColor(Color color)
        {
            m_CursorPool.SetColor(color);
        }

        static void UpdateCursorGameObject(IAnchor selectedAnchor, GameObject cursor)
        {
            if (selectedAnchor == null)
                return;

            cursor.transform.position = selectedAnchor.Position;
            cursor.transform.forward = selectedAnchor.Normal;

            if (!cursor.activeSelf)
            {
                cursor.gameObject.SetActive(true);
            }
        }
    }
}
