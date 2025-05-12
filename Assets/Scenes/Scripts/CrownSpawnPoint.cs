using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    public class CrownSpawnTrigger : MonoBehaviour
    {
        public GameObject Crown;
        public Transform SpawnLocation;
        public MedievalObjectives MedievalObjectives;
        
        void Start()
        {
            // Hide crown at start
            if (Crown)
                Crown.SetActive(false);
                
            // Make sure we have a trigger
            Collider col = GetComponent<Collider>();
            if (col)
                col.isTrigger = true;
                
            // Get reference to MedievalObjectives if not set
            if (MedievalObjectives == null)
                MedievalObjectives = FindObjectOfType<MedievalObjectives>();
        }
        
        void OnTriggerEnter(Collider other)
        {
            // If it's the player
            if (other.GetComponent<PlayerCharacterController>())
            {
                // Show crown at spawn location
                if (Crown && SpawnLocation)
                {
                    Crown.transform.position = SpawnLocation.position;
                    Crown.transform.rotation = SpawnLocation.rotation;
                    Crown.SetActive(true);
                    
                    // Notify the objectives system
                    if (MedievalObjectives != null)
                    {
                        MedievalObjectives.OnCrownRevealed();
                        
                        // Mark the current objective as complete and advance to next
                        MedievalObjectives.AdvanceToNextObjectiveAfterCrownRevealed();
                    }
                }
                
                // Disable this trigger
                gameObject.SetActive(false);
            }
        }
    }
}