using System;
using System.Collections;
using UnityEngine;

namespace Unity.ReferenceProject.VR.VRControls
{
    /// <summary>
    ///     Controls a blend shape animation based on the ray interactor select state. It also optionally plays an audio source
    ///     when the cursor starts selecting.
    /// </summary>
    public class CursorBlendShapeController : MonoBehaviour
    {
        const float k_SelectingBlendWeight = 0f;
        const float k_NotSelectingBlendWeight = 100f;

        [SerializeField, Tooltip("The ray interactor cursor")]
        RayInteractionCursor m_RayInteractorRenderer;

        [SerializeField, Tooltip("The skinned mesh renderer that has a blend shape to control for the cursor select animation.")]
        SkinnedMeshRenderer m_Blendshape;

        [SerializeField, Tooltip("The index of the blendshape that will be driven when the cursor is selecting.")]
        int m_BlendshapeIndex;

        [SerializeField, Tooltip("The duration of the blendshape transition animation when selecting.")]
        float m_BlendshapeTransitionDuration = 0.075f;
        float m_BlendWeightStart;

        Coroutine m_Routine;
        float m_TargetBlendWeight;

        void Awake()
        {
            m_RayInteractorRenderer.OnShow.AddListener(OnShow);
        }

        void LateUpdate()
        {
            if (m_RayInteractorRenderer.RayInteractor != null)
            {
                var selecting = m_RayInteractorRenderer.Selected;
                if (selecting)
                {
                    if (!Mathf.Approximately(m_TargetBlendWeight, k_SelectingBlendWeight))
                    {
                        m_BlendWeightStart = m_Blendshape.GetBlendShapeWeight(m_BlendshapeIndex);
                        m_TargetBlendWeight = k_SelectingBlendWeight;
                        StartBlendShapeAnimation();
                    }
                }
                else
                {
                    if (!Mathf.Approximately(m_TargetBlendWeight, k_NotSelectingBlendWeight))
                    {
                        m_BlendWeightStart = m_Blendshape.GetBlendShapeWeight(m_BlendshapeIndex);
                        m_TargetBlendWeight = k_NotSelectingBlendWeight;
                        StartBlendShapeAnimation();
                    }
                }
            }
        }

        void OnDestroy()
        {
            m_RayInteractorRenderer.OnShow.RemoveListener(OnShow);
        }

        void OnShow(bool show)
        {
            m_Blendshape.gameObject.SetActive(show);
        }

        void StartBlendShapeAnimation()
        {
            if (m_Routine != null)
            {
                StopCoroutine(m_Routine);
            }

            m_Routine = StartCoroutine(Interpolate());
        }

        IEnumerator Interpolate()
        {
            var startTime = Time.realtimeSinceStartup;
            var elapsed = 0f;
            while (elapsed <= m_BlendshapeTransitionDuration)
            {
                yield return null;

                elapsed = Time.realtimeSinceStartup - startTime;
                var t = elapsed / m_BlendshapeTransitionDuration;
                t = Mathf.Clamp(t, 0.0f, 1.0f);
                m_Blendshape.SetBlendShapeWeight(m_BlendshapeIndex, Mathf.Lerp(m_BlendWeightStart, m_TargetBlendWeight, t));
            }
        }
    }
}
