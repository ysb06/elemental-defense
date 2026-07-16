using System.Collections.Generic;
using UnityEngine;

namespace DefCity.Presentation.Visuals
{
    [DisallowMultipleComponent]
    public class RandomObjectActivator : MonoBehaviour
    {
        [SerializeField] private List<GameObject> objects = new();
        [SerializeField] private bool activateOnStart = true;

        public GameObject ActiveObject { get; private set; }

        private void Start()
        {
            if (activateOnStart)
            {
                ActivateRandom();
            }
        }

        public void ActivateRandom()
        {
            List<GameObject> validObjects = GetValidObjects();
            if (validObjects.Count == 0)
            {
                ActiveObject = null;
                Debug.LogError($"{nameof(RandomObjectActivator)} on {name} requires at least one valid object.", this);
                return;
            }

            ActivateOnly(validObjects[Random.Range(0, validObjects.Count)]);
        }

        public void ActivateOnly(GameObject target)
        {
            if (target == null || !objects.Contains(target))
            {
                ActiveObject = null;
                Debug.LogError($"{nameof(RandomObjectActivator)} on {name} cannot activate an object that is not in its list.", this);
                return;
            }

            foreach (GameObject candidate in objects)
            {
                if (candidate != null)
                {
                    candidate.SetActive(candidate == target);
                }
            }

            ActiveObject = target;
        }

        private List<GameObject> GetValidObjects()
        {
            List<GameObject> validObjects = new();
            foreach (GameObject candidate in objects)
            {
                if (candidate != null)
                {
                    validObjects.Add(candidate);
                }
            }

            return validObjects;
        }
    }
}
