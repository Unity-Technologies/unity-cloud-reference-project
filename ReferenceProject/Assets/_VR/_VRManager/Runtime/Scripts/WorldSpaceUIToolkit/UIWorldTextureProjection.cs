using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.VRManager
{
    public class UIWorldTextureProjection : MonoBehaviour
    {
        [SerializeField]
        UIXRCursor[] m_CursorObjects;

        [SerializeField]
        PanelSettings m_TargetPanel;

        Func<Vector2, Vector2> m_DefaultRenderTextureScreenTranslation;

        public PanelSettings TargetPanel => m_TargetPanel;

        void OnEnable()
        {
            if (TargetPanel != null)
            {
                if (m_DefaultRenderTextureScreenTranslation == null)
                {
                    m_DefaultRenderTextureScreenTranslation = pos => ScreenCoordinatesToRenderTexture(pos);
                }

                TargetPanel.SetScreenToPanelSpaceFunction(m_DefaultRenderTextureScreenTranslation);
            }

            m_CursorObjects = FindObjectsOfType<UIXRCursor>();
        }

        void OnDisable()
        {
            //we reset it back to the default behavior
            if (TargetPanel != null)
            {
                TargetPanel.SetScreenToPanelSpaceFunction(null);
            }
        }

        /// <summary>
        ///     Transforms a screen position to a position relative to render texture used by a MeshRenderer.
        /// </summary>
        /// <param name="screenPosition">The position in screen coordinates.</param>
        /// <param name="currentCamera">Camera used for 3d object picking</param>
        /// <param name="targetTexture">The texture used by the panel</param>
        /// <returns>
        ///     Returns the coordinates in texel space, or a position containing NaN values if no hit was recorded or if the
        ///     hit mesh's material is not using the render texture as their mainTexture
        /// </returns>
        Vector2 ScreenCoordinatesToRenderTexture(Vector2 screenPosition)
        {
            var invalidPosition = new Vector2(float.NaN, float.NaN);

            screenPosition.y = Screen.height - screenPosition.y;

            RaycastHit hit;
            MeshRenderer rend = null;
            var hasHitThisFrame = false;

            var cursorRay = new Ray(m_CursorObjects[0].Position, m_CursorObjects[0].Direction);
            Debug.DrawRay(m_CursorObjects[0].Position, m_CursorObjects[0].Direction, Color.blue, 5f);
            if (Physics.Raycast(cursorRay, out hit))
            {
                Debug.Log($"Cursor Hit {hit.transform.name}", hit.transform);
                hasHitThisFrame |= true;
                rend = hit.transform.GetComponent<MeshRenderer>();
            }

            if (!hasHitThisFrame)
            {
                return invalidPosition;
            }

            var targetTexture = TargetPanel.targetTexture;

            if (rend == null || rend.sharedMaterial.mainTexture != targetTexture)
            {
                return invalidPosition;
            }

            var pixelUV = hit.textureCoord;
            pixelUV.y = 1 - pixelUV.y;
            pixelUV.x *= targetTexture.width;
            pixelUV.y *= targetTexture.height;

            return pixelUV;
        }
    }
}
