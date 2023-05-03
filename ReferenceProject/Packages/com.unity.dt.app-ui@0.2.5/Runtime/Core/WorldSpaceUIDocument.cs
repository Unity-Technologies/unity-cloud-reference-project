using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.Core
{
    /// <summary>
    /// A <see cref="MonoBehaviour"/> which allows to display a <see cref="UIDocument"/> in World-Space.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class WorldSpaceUIDocument : MonoBehaviour
    {
        /// <summary>
        /// The camera used to raycast against World-Space UI Panels.
        /// <remarks>
        /// You can also use <see cref="customRayFunc"/> property to use a custom Ray computation method.
        /// </remarks>
        /// </summary>
        [Tooltip("The camera used to raycast against WorldSpace UI Panels.\n" +
            "You can also use WorldSpaceUIDocument.customRayFunc property to use a custom Ray computation method.")]
        public Camera targetCamera;

        /// <summary>
        /// The LayerMask used with raycasts against World-Space UI Panels.
        /// </summary>
        [Tooltip("The LayerMask used with raycasts against World-Space UI Panels.")]
        public LayerMask layerMask;

        /// <summary>
        /// A custom method to compute a Ray used as reference to raycast against World-Space UI Panels.
        /// </summary>
        public Func<Ray> customRayFunc;

        PanelSettings m_PanelSettings;

        void OnEnable()
        {
            m_PanelSettings = GetComponent<UIDocument>().panelSettings;
            if (m_PanelSettings)
                m_PanelSettings.SetScreenToPanelSpaceFunction(ScreenCoordinatesToRenderTexture);
        }

        void OnDisable()
        {
            if (m_PanelSettings)
                m_PanelSettings.SetScreenToPanelSpaceFunction(null);
        }

        Vector2 ScreenCoordinatesToRenderTexture(Vector2 screenPosition)
        {
            var invalidPosition = new Vector2(float.NaN, float.NaN);

            if (customRayFunc == null && !targetCamera)
                return invalidPosition;

            screenPosition.y = Screen.height - screenPosition.y;
            var cameraRay = customRayFunc?.Invoke() ?? targetCamera.ScreenPointToRay(screenPosition);
            var dist = customRayFunc != null ? cameraRay.direction.magnitude : targetCamera.farClipPlane;

            if (!Physics.Raycast(cameraRay, out var hit, dist, layerMask))
                return invalidPosition;

            var targetTexture = m_PanelSettings.targetTexture;
            var rend = hit.transform.GetComponent<MeshRenderer>();

            if (!rend || !rend.sharedMaterial || rend.sharedMaterial.mainTexture != targetTexture)
                return invalidPosition;

            var pixelUV = hit.textureCoord;

            //since y screen coordinates are usually inverted, we need to flip them
            pixelUV.y = 1 - pixelUV.y;

            pixelUV.x *= targetTexture.width;
            pixelUV.y *= targetTexture.height;

            return pixelUV;
        }
    }
}
