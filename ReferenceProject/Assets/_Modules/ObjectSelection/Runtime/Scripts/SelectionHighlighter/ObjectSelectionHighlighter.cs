using System;
using System.Collections.Generic;
using Unity.ReferenceProject.DataStores;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.ObjectSelection
{
    public class ObjectSelectionHighlighter : MonoBehaviour
    {
        [SerializeField]
        bool m_IsRemoveSubMeshes = true;

        [SerializeField]
        Material m_Material;

        PropertyValue<IObjectSelectionInfo> m_ObjectSelectionProperty;

        MeshFilter m_OutlineMeshFilter;
        MeshRenderer m_OutlineMeshRenderer;

        [Inject]
        void Setup(PropertyValue<IObjectSelectionInfo> objectSelectionProperty)
        {
            m_ObjectSelectionProperty = objectSelectionProperty;
            m_ObjectSelectionProperty.ValueChanged -= OnObjectSelectionChanged;
            m_ObjectSelectionProperty.ValueChanged += OnObjectSelectionChanged;
        }

        void Start()
        {
            CreateDummy();
        }

        void OnDestroy()
        {
            if (m_ObjectSelectionProperty != null)
                m_ObjectSelectionProperty.ValueChanged -= OnObjectSelectionChanged;
        }

        void OnObjectSelectionChanged(IObjectSelectionInfo obj) => SetTargetGameObject(obj.SelectedGameObject);

        void SetTargetGameObject(GameObject target)
        {
            if (!target)
            {
                if (m_OutlineMeshFilter)
                {
                    m_OutlineMeshFilter.gameObject.SetActive(false);
                }

                return;
            }

            var targetMeshFilter = target.GetComponent<MeshFilter>();
            var targetMeshRenderer = target.GetComponent<MeshRenderer>();

            if (!targetMeshFilter || !targetMeshRenderer)
            {
                if (m_OutlineMeshFilter != null)
                {
                    m_OutlineMeshFilter.gameObject.SetActive(false);
                }
            }
            else
            {
                if (m_OutlineMeshFilter == null)
                {
                    CreateDummy();
                }

                ReplaceMaterials(target, targetMeshRenderer, targetMeshFilter);

                m_OutlineMeshFilter.gameObject.SetActive(true);
            }
        }

        void ReplaceMaterials(GameObject target, Renderer targetMeshRenderer, MeshFilter targetMeshFilter)
        {
            var t = m_OutlineMeshFilter.transform;
            t.SetPositionAndRotation(target.transform.position, target.transform.rotation);
            t.localScale = target.transform.localScale;

            if (m_IsRemoveSubMeshes)
            {
                m_OutlineMeshFilter.mesh = targetMeshRenderer.materials.Length > 1
                    ? RemoveSubMeshes(Instantiate(targetMeshFilter.mesh))
                    : targetMeshFilter.mesh;
                m_OutlineMeshRenderer.material = m_Material;
            }
            else
            {
                m_OutlineMeshFilter.mesh = targetMeshFilter.mesh;

                var materials = targetMeshRenderer.materials;

                for (var i = 0; i < materials.Length; i++)
                {
                    materials[i] = m_Material;
                }

                m_OutlineMeshRenderer.materials = materials;
            }
        }

        /// <summary>
        ///     Make mesh with many materials to mesh with one material
        /// </summary>
        Mesh RemoveSubMeshes(Mesh mesh)
        {
            var triIndices = new List<int>();
            for (var i = 0; i < mesh.subMeshCount; i++)
            {
                var indices = mesh.GetIndices(i);
                triIndices.AddRange(indices);
            }

            var oneTrueSubMesh = mesh.GetSubMesh(0);
            oneTrueSubMesh.firstVertex = oneTrueSubMesh.baseVertex = 0;
            oneTrueSubMesh.vertexCount = mesh.vertexCount;
            oneTrueSubMesh.indexStart = 0;
            oneTrueSubMesh.indexCount = triIndices.Count;

            for (var i = mesh.subMeshCount - 1; i > 0; i--)
            {
                var subMesh = mesh.GetSubMesh(i);
                subMesh.vertexCount = 0;
                subMesh.indexStart = 0;
                subMesh.firstVertex = 0;
                subMesh.indexCount = 0;
                mesh.SetSubMesh(i, subMesh);
            }

            mesh.SetSubMesh(0, oneTrueSubMesh);
            mesh.subMeshCount = 1;

            return mesh;
        }

        void CreateDummy()
        {
            var go = new GameObject("Outline");
            go.layer = 6;
            go.transform.SetParent(transform);
            m_OutlineMeshFilter = go.AddComponent<MeshFilter>();
            m_OutlineMeshRenderer = go.AddComponent<MeshRenderer>();
            go.SetActive(false);
        }
    }
}
