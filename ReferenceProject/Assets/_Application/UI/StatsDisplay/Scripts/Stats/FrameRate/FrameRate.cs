using System;
using UnityEngine;
using Unity.ReferenceProject.Stats;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject
{
    public class FrameRate : TextStat
    {
        [SerializeField]
        float m_TargetFrameRate = 60.0f;

        [SerializeField]
        Gradient m_ColorGradient;

        [SerializeField]
        FrameRateCalculator m_FrameRateCalculator = new();

        VisualElement m_RootVisualElement;

        void Awake()
        {
            m_FrameRateCalculator.FrameRateRefreshed += OnFrameRateRefreshed;
        }

        protected override void OnStatUpdate()
        {
            m_FrameRateCalculator.Update(Time.deltaTime);
        }

        void OnFrameRateRefreshed(int fps, int minFps, int maxFps)
        {
            var color = ColorUtility.ToHtmlStringRGBA(m_ColorGradient.Evaluate(fps / m_TargetFrameRate));
            var minColor = ColorUtility.ToHtmlStringRGBA(m_ColorGradient.Evaluate(minFps / m_TargetFrameRate));
            var maxColor = ColorUtility.ToHtmlStringRGBA(m_ColorGradient.Evaluate(maxFps / m_TargetFrameRate));

            Text = $"FPS <color=#{color}>{fps}</color>\t<size=-3>Min <color=#{minColor}>{minFps}</color>  Max <color=#{maxColor}>{maxFps}</color></size>";
        }
    }
}
