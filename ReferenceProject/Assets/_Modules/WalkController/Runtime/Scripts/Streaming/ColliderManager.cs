using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.ReferenceProject.Messaging;
using Unity.ReferenceProject.ObjectSelection;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.WalkController
{
    public class ColliderManager : MonoBehaviour
    {
        [SerializeField, Tooltip("Automatically remove colliders if they are outside CheckRadius")]
        bool m_IsRemoveColliders;

        [SerializeField, Tooltip("Colliders will be created for GameObjects within this radius")]
        float m_CheckRadius = 8;

        [SerializeField]
        float m_FrequencyFloorCheck = 0.03f;
        readonly Dictionary<GameObject, HashSet<MeshCollider>> m_ColliderCache = new();
        readonly MeshColliderPool m_ColliderPool = new();

        readonly List<MeshFilter> m_MeshFilters = new();

        readonly HashSet<GameObject> m_ObjectsToAdd = new();

        IFirstPersonMoveController m_FirstPersonMoveController;

        GameObject m_GroundGameObject;

        IObjectPicker m_Picker;

        Task m_Task;
        Task m_TaskFloor;

        float m_Time;

        IAppMessaging m_AppMessaging;

        [Inject]
        public void Setup(IObjectPicker picker, IAppMessaging appMessaging)
        {
            m_Picker = picker;
            m_AppMessaging = appMessaging;
        }

        void Awake()
        {
            m_FirstPersonMoveController = GetComponent<IFirstPersonMoveController>();

            if (GetComponent<IFirstPersonMoveController>() == null)
            {
                Debug.LogError($"MonoBehaviour with interface {nameof(IFirstPersonMoveController)} doesn't exist on {gameObject.name}");
            }
            
            m_AppMessaging.ShowDialog("Incomplete Feature", "Walk Mode relies on com.unity.cloud.data-streaming's Object Picker, which is not implemented yet in current version. " +
                "Collision detection with objects will not work.", "Ok");
        }

        void Update()
        {
            if (m_Picker == null)
                return;

            // Surrounding check and add colliders
            if (m_Task?.IsCompleted ?? true)
            {
                m_Task = GetSurroundingAsyncCollider();
                if (m_IsRemoveColliders)
                    RemoveColliders();
                AddColliders(m_ObjectsToAdd, m_GroundGameObject);
            }

            // Additional ground check.
            // if player jumps from the roof, he should fall.
            // if player jumps into space without ground at all, than he should not fall.
            // also, it will add a collider to the ground to prevent the pass through the ground from falling at high
            // speed when the GetSurroundingAsyncCollider() is still pending.
            if (m_Time < m_FrequencyFloorCheck)
            {
                m_Time += Time.deltaTime;
            }
            else if (m_TaskFloor?.IsCompleted ?? true)
            {
                m_TaskFloor = GetFloorAsyncCollider();
                m_Time = 0;
            }
        }

        void OnEnable()
        {
            if (m_FirstPersonMoveController != null)
                m_FirstPersonMoveController.useGravity = false;
        }

        void OnDisable()
        {
            Clean();
        }

        void OnDrawGizmosSelected()
        {
            // Drawing m_CheckRadius sphere
            Gizmos.color = new Color(0, 1, 0, 0.1f);
            Gizmos.DrawSphere(transform.position, m_CheckRadius);
        }

        internal int CountColliderCache(GameObject key) =>
            m_ColliderCache.ContainsKey(key) ? m_ColliderCache[key].Count : 0; // For automatic tests

        internal void AddColliders(HashSet<GameObject> objectsToAdd, GameObject groundGameObject)
        {
            if (groundGameObject != null && !objectsToAdd.Contains(groundGameObject))
                objectsToAdd.Add(groundGameObject);

            // Add the new element close to the player to the cash and add the collider except for the door
            foreach (var go in objectsToAdd)
            {
                // Check if the gameobject already have a collider
                if (go == null || m_ColliderCache.ContainsKey(go))
                    return;

                // add new list to cache since we know it doesn't already exist due to previous check
                // even if we don't add any colliders this will prevent the obj from being processed next time
                if (!m_ColliderCache.ContainsKey(go))
                    m_ColliderCache.Add(go, new HashSet<MeshCollider>());

                var colliderSet = m_ColliderCache[go];

                m_MeshFilters.Clear();
                go.GetComponentsInChildren(m_MeshFilters);

                foreach (var meshFilter in m_MeshFilters)
                {
                    var collider = m_ColliderPool.GetMeshCollider();
                    collider.enabled = false;
                    collider.sharedMesh = meshFilter.sharedMesh;
                    collider.transform.SetPositionAndRotation(meshFilter.transform.position, meshFilter.transform.rotation);
                    collider.transform.localScale = meshFilter.transform.localScale;
                    colliderSet.Add(collider);
                    collider.enabled = true;
                }
            }

            objectsToAdd.Clear();
        }

        internal void RemoveColliders(HashSet<GameObject> ignoreThisObjects = null)
        {
            if (m_ColliderCache.Count == 0)
                return;

            var objectsToRemove = new HashSet<GameObject>(m_ColliderCache.Keys);
            if (ignoreThisObjects != null)
                objectsToRemove.ExceptWith(ignoreThisObjects);

            // Remove the old collider from the gameobject that are far from the character
            foreach (var go in objectsToRemove)
            {
                if (go == null || !m_ColliderCache.ContainsKey(go))
                    return;

                foreach (var meshCollider in m_ColliderCache[go])
                    m_ColliderPool.ReleaseMeshCollider(meshCollider);

                m_ColliderCache.Remove(go);
            }
        }

        async Task GetFloorAsyncCollider()
        {
            var ray = new Ray(transform.position + Vector3.up * 0.3f, Vector3.down);
            var result = await m_Picker.PickFromRayAsync(ray);
            if (m_FirstPersonMoveController != null)
                m_FirstPersonMoveController.useGravity = result.Count > 0;
            m_GroundGameObject = result.Count > 0 ? result[0].Item1 : null;
        }

        async Task GetSurroundingAsyncCollider()
        {
            var list = await m_Picker.PickFromSphereAsync(transform.position, m_CheckRadius);
            foreach (var go in list)
                m_ObjectsToAdd.Add(go.Item1);
        }

        void Clean()
        {
            foreach (var go in m_ColliderCache)
            {
                if (!go.Key)
                    continue;

                foreach (var c in go.Value)
                    Destroy(c);
            }

            m_ColliderCache.Clear();
            m_ColliderPool.Clear();
        }

        class MeshColliderPool
        {
            readonly Stack<MeshCollider> m_Pool = new();

            Transform poolRoot;
            public int Count => m_Pool.Count;

            public MeshCollider GetMeshCollider()
            {
                if (!poolRoot)
                    poolRoot = new GameObject("Pool_MeshCollider").transform;

                if (m_Pool.Count == 0)
                {
                    var collider = new GameObject().AddComponent<MeshCollider>();
                    collider.transform.SetParent(poolRoot);
                    collider.name = "";
                    m_Pool.Push(collider);
                }

                m_Pool.Peek().enabled = true;
                return m_Pool.Pop();
            }

            public void ReleaseMeshCollider(MeshCollider meshCollider)
            {
                m_Pool.Push(meshCollider);
                meshCollider.sharedMesh = null;
                meshCollider.enabled = false;
            }

            public void Clear()
            {
                if (poolRoot)
                    Destroy(poolRoot.gameObject);

                m_Pool.Clear();
            }
        }
    }
}
