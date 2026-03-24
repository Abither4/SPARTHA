using UnityEngine;

namespace Spartha.World
{
    public class RandomEncounter : MonoBehaviour
    {
        public float stepCounter;
        public float encounterRate = 0.08f; // 8% per step check
        public float stepCheckDistance = 2f;
        public bool inGrass;

        private Vector3 lastCheckPos;
        private OverworldUI overworldUI;

        void Start()
        {
            lastCheckPos = transform.position;
            overworldUI = Object.FindAnyObjectByType<OverworldUI>();
        }

        void Update()
        {
            if (!inGrass) return;

            float dist = Vector3.Distance(transform.position, lastCheckPos);
            if (dist >= stepCheckDistance)
            {
                lastCheckPos = transform.position;
                stepCounter++;

                if (Random.value < encounterRate)
                {
                    TriggerEncounter();
                }
            }
        }

        void TriggerEncounter()
        {
            if (overworldUI != null)
                overworldUI.ShowEncounter();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("TallGrass"))
                inGrass = true;
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("TallGrass"))
                inGrass = false;
        }
    }
}
