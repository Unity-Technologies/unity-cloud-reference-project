using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class will stockpile a bunch of objects that are deactivated for later use.
/// This allows us to use TONS of objects fast, instead of instantiating hundreds at once.
/// </summary>
namespace DataStructures.ViliWonka.Heap
{
    public class Heap : MonoBehaviour
    {
        #region Variables

        #region Inspector Values

        //The object to stockpile
        [SerializeField] [Tooltip("The object to stockpile")]
        private GameObject prefab;

        //The amount of objects to stockpile
        [SerializeField] [Tooltip("The amount of objects to stockpile")]
        private int size = 100;

        [SerializeField] [Tooltip("How much to increase the heap if it goes over max.")]
        private int sizeIncrease = 50;

        #endregion

        #region Other

        //The collection of objects stockpiled
        private readonly List<GameObject> deactive = new List<GameObject>();
        private readonly List<GameObject> active = new List<GameObject>();

        #endregion

        #region Properties

        public int Total
        {
            get
            {
                if (deactive.Count > 0 && active.Count > 0)
                    return deactive.Count + active.Count;
                else
                    return 0;
            }
        }

        public int CurrentCount
        {
            get { return deactive.Count; }
        }

        public int ActiveCount
        {
            get { return active.Count; }
        }

        #endregion

        #endregion

        #region Methods

        #region Monobehaviour Method Implementation

        // Use this for initialization
        void Start()
        {
            //Generate all the objects in the heap and deactivate them
            for (int i = 0; i < size; i++)
            {
                GameObject obj = Instantiate(prefab, new Vector3(), Quaternion.identity, gameObject.transform);
                obj.gameObject.SetActive(false);
                deactive.Add(obj);
            }
        }

        #endregion

        #region Class Method Implementation

        /// <summary>
        /// Grabs the next available object in the heap for use.
        /// </summary>
        /// <returns>The next usable.</returns>
        public GameObject GetNextUsable()
        {
            if (deactive.Count == 0)
                SpawnSet();

            GameObject obj = deactive[0];
            active.Add(deactive[0]);
            deactive.RemoveAt(0);

            return obj;
        }

        public void ReturnObject(GameObject obj)
        {
            if (!active.Contains(obj))
            {
                Debug.LogWarning("You can only return an object that belongs to the heap.", obj.gameObject);
                return;
            }

            deactive.Add(obj);
            obj.gameObject.SetActive(false);
            obj.transform.parent = transform;
            active.Remove(obj);
        }

        void SpawnSet()
        {
            //Generate all the objects in the heap and deactivate them
            for (int i = 0; i < sizeIncrease; i++)
            {
                GameObject obj = Instantiate(prefab, new Vector3(), Quaternion.identity, gameObject.transform);
                obj.gameObject.SetActive(false);
                deactive.Add(obj);
            }
        }

        public void Reset()
        {
            if (active == null)
                return;

            while (active.Count > 0)
            {
                deactive.Add(active[0]);
                active[0].gameObject.SetActive(false);
                active[0].transform.parent = transform;
                active.RemoveAt(0);
            }
        }

        #endregion

        #endregion
    }
}
