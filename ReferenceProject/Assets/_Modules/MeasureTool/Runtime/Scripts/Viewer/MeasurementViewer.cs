using System;
using System.Collections.Generic;
using Unity.ReferenceProject.DataStores;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.MeasureTool
{
    public class MeasurementViewer : MonoBehaviour
    {
        [SerializeField]
        MeasureSegment m_MeasureSegmentPrefab;

        PropertyValue<MeasureFormat> m_MeasureFormat;

        DiContainer m_DiContainer;

        struct MeasurementInfo
        {
            public MeasureSegment measureSegment;
            public MeasureLineData Data;
        }

        readonly Dictionary<string, MeasurementInfo> m_Measurements = new();

        [Inject]
        public void Setup(MeasureToolDataStore dataStore, DiContainer container)
        {
            m_MeasureFormat = dataStore.GetProperty<MeasureFormat>(nameof(MeasureToolViewModel.MeasureFormat));
            m_DiContainer = container;
        }

        void OnEnable()
        {
            m_MeasureFormat.ValueChanged += OnMeasureFormatChanged;
        }

        void OnDisable()
        {
            m_MeasureFormat.ValueChanged -= OnMeasureFormatChanged;
        }

        public void AddMeasureLineData(MeasureLineData targetData)
        {
            if (targetData == null)
                return;

            var measurementInfo = new MeasurementInfo
            {
                Data = targetData,
                measureSegment = m_Measurements.TryGetValue(targetData.Id, out var info) ? info.measureSegment : m_DiContainer.InstantiatePrefabForComponent<MeasureSegment>(m_MeasureSegmentPrefab)
            };

            m_Measurements[targetData.Id] = measurementInfo;

            ApplyMeasureLineData(measurementInfo.measureSegment, measurementInfo.Data, m_MeasureFormat.GetValue());
        }

        void OnMeasureFormatChanged(MeasureFormat format)
        {
            foreach (var info in m_Measurements.Values)
            {
                info.measureSegment.SetLabelText(info.Data.GetFormattedDistanceString(format));
            }
        }

        void OnMeasureLineDataChanged(MeasureSegment measureSegment, MeasureLineData data)
        {
            ApplyMeasureLineData(measureSegment, data, m_MeasureFormat.GetValue());
            measureSegment.SetVisible(data.Anchors.Count >= 2);
        }

        static void ApplyMeasureLineData(MeasureSegment measureSegment, MeasureLineData data, MeasureFormat format)
        {
            if (data.Anchors.Count >= 2)
            {
                measureSegment.StartPosition = data.Anchors[0].Position;
                measureSegment.EndPosition = data.Anchors[1].Position;
                measureSegment.SetLabelText(data.GetFormattedDistanceString(format));
                measureSegment.SetColor(data.Color);
            }
        }

        void Update()
        {
            foreach (var info in m_Measurements.Values)
            {
                OnMeasureLineDataChanged(info.measureSegment, info.Data);
            }
        }

        public void RemoveMeasureLineData(MeasureLineData lineData)
        {
            if (m_Measurements.TryGetValue(lineData.Id, out var info))
            {
                m_Measurements.Remove(lineData.Id);
                Destroy(info.measureSegment.gameObject);
            }
        }

        public void RemoveAllMeasureLineData()
        {
            foreach (var info in m_Measurements.Values)
            {
                Destroy(info.measureSegment.gameObject);
            }

            m_Measurements.Clear();
        }
    }
}
