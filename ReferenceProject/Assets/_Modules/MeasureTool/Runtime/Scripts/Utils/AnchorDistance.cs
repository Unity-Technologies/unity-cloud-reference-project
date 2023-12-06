using System.Globalization;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool.Utils
{
    public static class AnchorDistance
    {
        public static float GetDistanceBetweenAnchorsMeters(IAnchor anc1, IAnchor anc2)
        {
            return Vector3.Distance(anc1.Position, anc2.Position);
        }

        public static string GetDistanceBetweenAnchorsString(IAnchor anc1, IAnchor anc2)
        {
            var distance = GetDistanceBetweenAnchorsMeters(anc1, anc2);
            return distance.ToString(CultureInfo.InvariantCulture) + "m";
        }
    }
}
