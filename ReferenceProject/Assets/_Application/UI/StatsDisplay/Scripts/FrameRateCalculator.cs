using System;
using UnityEngine;

namespace Unity.ReferenceProject
{
    [Serializable]
    public class FrameRateCalculator
    {
        public delegate void FrameRateRefreshDelegate(int fps, int minFps, int maxFps);

        [SerializeField]
        float m_UpdateFrequency = 1.0f;

        [SerializeField]
        int m_FrameBufferCount = 30;
        int m_CurrentIndex;
        int m_CurrentValidFrameCount;

        float[] m_FrameCounts;

        float m_FrameRate;
        float m_MaxFrameRate;
        float m_MinFrameRate;

        float m_Timer;
        float m_TotalFrameRate;

        public FrameRateCalculator()
        {
            m_FrameCounts = new float[m_FrameBufferCount];
            for (var i = 0; i < m_FrameCounts.Length; ++i)
            {
                m_FrameCounts[i] = -1;
            }
        }

        public event FrameRateRefreshDelegate FrameRateRefreshed;

        public void Update(float deltaTime)
        {
            m_FrameCounts[m_CurrentIndex] = 1f / deltaTime;
            ++m_CurrentIndex;
            m_CurrentIndex %= m_FrameCounts.Length;

            m_Timer -= deltaTime;

            if (m_Timer <= 0.0f)
            {
                m_Timer = m_UpdateFrequency;
                Calculate();
                FrameRateRefreshed?.Invoke(Mathf.RoundToInt(m_FrameRate), Mathf.RoundToInt(m_MinFrameRate), Mathf.RoundToInt(m_MaxFrameRate));
            }
        }

        void Calculate()
        {
            m_CurrentValidFrameCount = 0;
            m_TotalFrameRate = 0;
            m_MinFrameRate = float.MaxValue;
            m_MaxFrameRate = float.MinValue;
            for (var i = 0; i < m_FrameCounts.Length; ++i)
            {
                var value = m_FrameCounts[i];
                if (value <= 0)
                    continue;

                ++m_CurrentValidFrameCount;
                m_TotalFrameRate += value;

                if (m_MinFrameRate > value) m_MinFrameRate = value;
                if (m_MaxFrameRate < value) m_MaxFrameRate = value;
            }

            if (m_CurrentValidFrameCount > 0)
                m_FrameRate = m_TotalFrameRate / m_CurrentValidFrameCount;
        }
    }
}
