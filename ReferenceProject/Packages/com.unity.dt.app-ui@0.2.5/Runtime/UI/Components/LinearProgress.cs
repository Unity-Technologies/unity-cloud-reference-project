using UnityEngine.Dt.App.Core;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// LinearProgress UI element.
    /// </summary>
    public class LinearProgress : Progress
    {
        static readonly int k_Start = Shader.PropertyToID("_Start");

        static readonly int k_End = Shader.PropertyToID("_End");

        static readonly int k_BufferStart = Shader.PropertyToID("_BufferStart");

        static readonly int k_BufferEnd = Shader.PropertyToID("_BufferEnd");

        static readonly int k_Color = Shader.PropertyToID("_Color");

        static readonly int k_AA = Shader.PropertyToID("_AA");

        static readonly int k_Ratio = Shader.PropertyToID("_Ratio");

        static readonly int k_Padding = Shader.PropertyToID("_Padding");

        static readonly int k_BufferOpacity = Shader.PropertyToID("_BufferOpacity");

        static Material s_Material;

        /// <summary>
        /// The Progress main styling class.
        /// </summary>
        public new static readonly string ussClassName = "appui-linear-progress";

        static readonly int k_Phase = Shader.PropertyToID("_Phase");

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LinearProgress()
        {
            AddToClassList(ussClassName);
        }

        /// <summary>
        /// Generates the textures for the progress element.
        /// </summary>
        protected override void GenerateTextures()
        {
            var rect = contentRect;

            if (!rect.IsValid())
                return;

            var dpi = Mathf.Max(Platform.mainScreenScale, 1f);
            var rectSize = rect.size * dpi;

            if (!rectSize.IsValidForTextureSize())
                return;

            if (m_RT && (Mathf.Abs(m_RT.width - rectSize.x) > 1 || Mathf.Abs(m_RT.height - rectSize.y) > 1))
            {
                m_RT.Release();
                m_RT = null;
            }

            if (!m_RT)
            {
                m_RT = new RenderTexture((int)rectSize.x, (int)rectSize.y, 24);
                m_RT.Create();
            }

            if (!s_Material)
            {
                s_Material = new Material(Shader.Find("Hidden/App UI/LinearProgress"));
            }

            var time = Application.isEditor ?
#if UNITY_EDITOR
                (float)EditorApplication.timeSinceStartup
#else
                Time.time
#endif
                : Time.time;

            s_Material.SetColor(k_Color, colorOverride ?? m_Color);
            s_Material.SetFloat(k_Start, 0);
            s_Material.SetFloat(k_End, value);
            s_Material.SetFloat(k_BufferStart, 0);
            s_Material.SetFloat(k_BufferEnd, bufferValue);
            s_Material.SetFloat(k_BufferOpacity, bufferOpacity);
            s_Material.SetFloat(k_AA, 2.0f / rectSize.x);
            s_Material.SetVector(k_Phase, new Vector4(time / 20, time, time * 2, time * 3));
            s_Material.SetFloat(k_Ratio, rectSize.x / rectSize.y);
            s_Material.SetFloat(k_Padding, rect.height * 0.5f / rect.width);
            if (variant == Variant.Indeterminate)
                s_Material.EnableKeyword("PROGRESS_INDETERMINATE");
            else
                s_Material.DisableKeyword("PROGRESS_INDETERMINATE");

            var prevRt = RenderTexture.active;
            Graphics.Blit(null, m_RT, s_Material);
            RenderTexture.active = prevRt;
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="LinearProgress"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<LinearProgress, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="LinearProgress"/>.
        /// </summary>
        public new class UxmlTraits : Progress.UxmlTraits { }
    }
}
