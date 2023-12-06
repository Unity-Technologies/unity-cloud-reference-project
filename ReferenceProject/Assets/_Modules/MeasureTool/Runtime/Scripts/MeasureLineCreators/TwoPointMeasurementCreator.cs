using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool
{
    public class TwoPointMeasurementCreator : CursorsController
    {
        public override void CreateAnchorAtWorldPosition(Vector3 worldPosition, Vector3 normal)
        {
            if (m_SelectedLine.Anchors.Count >= 2)
            {
                Clear();
            }

            base.CreateAnchorAtWorldPosition(worldPosition, normal);
        }
    }
}