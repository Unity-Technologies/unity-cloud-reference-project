using System;
using Unity.ReferenceProject.DataStores;
using UnityEngine;
using Unity.Cloud.Common;
using Zenject;

namespace Unity.ReferenceProject.ObjectSelection
{
    public class ObjectSelectionReticule : MonoBehaviour
    {
        public static readonly string Layer = "ObjectSelection";
        
        [SerializeField]
        float m_IndicatorScale = 0.025f;
        
        [SerializeField]
        Material m_Material;
        
        GameObject m_Indicator;

        ObjectSelectionHighlightActivator m_ObjectSelectionHighlightActivator;
        PropertyValue<IObjectSelectionInfo> m_ObjectSelectionProperty;

        [Inject]
        void Setup(ObjectSelectionHighlightActivator objectSelectionHighlightActivator, PropertyValue<IObjectSelectionInfo> objectSelectionProperty)
        {
            m_ObjectSelectionProperty = objectSelectionProperty;
            
            m_ObjectSelectionHighlightActivator = objectSelectionHighlightActivator;
            objectSelectionHighlightActivator.OnActivate += OnActivate;
        }

        void OnDestroy()
        {
            if(m_ObjectSelectionProperty != null)
                m_ObjectSelectionProperty.ValueChanged -= OnObjectSelectionChanged;
            
            if(m_ObjectSelectionHighlightActivator != null)
                m_ObjectSelectionHighlightActivator.OnActivate -= OnActivate;
        }
        
        void Start()
        {
            m_Indicator = CreateDummy();
            m_Indicator.SetActive(false);
        }

        void OnActivate(bool isActive)
        {
            if (isActive)
            {
                m_ObjectSelectionProperty.ValueChanged -= OnObjectSelectionChanged;
                m_ObjectSelectionProperty.ValueChanged += OnObjectSelectionChanged;
                
                // Refresh state
                OnObjectSelectionChanged(m_ObjectSelectionProperty.GetValue());
            }
            else
            {
                m_ObjectSelectionProperty.ValueChanged -= OnObjectSelectionChanged;
            }
        }

        void OnObjectSelectionChanged(IObjectSelectionInfo info)
        { 
            if(m_Indicator == null)
                return;
        
            if(info.SelectedInstanceId != InstanceId.None)
            {
                m_Indicator.SetActive(true);
                m_Indicator.transform.position = info.SelectedPosition;
            }
            else
            {
                m_Indicator.SetActive(false);
            }
        }
        
        GameObject CreateDummy()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "ObjectSelectionReticule";
            go.transform.localScale = Vector3.one * m_IndicatorScale;
            go.layer = LayerMask.NameToLayer(Layer);
            go.transform.SetParent(transform);
            
            if (go.GetComponent<Collider>() is { } colliderComponent)
            {
                colliderComponent.enabled = false;
            }

            go.GetComponent<MeshRenderer>().material = m_Material;
            go.SetActive(false);
            return go;
        }
    }
}
