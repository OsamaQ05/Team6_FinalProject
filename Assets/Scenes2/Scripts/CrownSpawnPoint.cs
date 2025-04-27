using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class CrownSpawnPoint : MonoBehaviour
    {
        [Tooltip("The crown GameObject that will be activated")]
        public GameObject Crown;
        
        [Tooltip("Where the crown will appear")]
        public Transform CrownSpawnLocation;
        
        [Tooltip("Sound to play when crown appears")]
        public AudioClip CrownAppearSound;
        
        [Tooltip("Particle effect to play when crown appears")]
        public GameObject CrownAppearEffect;
        
        private bool hasTriggered = false;
        private MedievalManager medievalManager;
        private MedievalObjectives medievalObjectives;
        
        void Start()
        {
            // Get references
            medievalManager = FindObjectOfType<MedievalManager>();
            medievalObjectives = FindObjectOfType<MedievalObjectives>();
            
            // Make sure the crown is initially inactive
            if (Crown != null)
                Crown.SetActive(false);
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (hasTriggered)
                return;
                
            var player = other.GetComponent<PlayerCharacterController>();
            
            // Check if it's the player entering the trigger
            if (player != null)
            {
                hasTriggered = true;
                
                // Spawn the crown at the designated location
                if (Crown != null && CrownSpawnLocation != null)
                {
                    // Debug log to verify this code is running
                    Debug.Log("Moving crown to spawn location: " + CrownSpawnLocation.position);
                    
                    // First make sure the crown is inactive before moving it
                    Crown.SetActive(false);
                    
                    // Set the crown's position and rotation - explicitly using transform to ensure it works
                    Crown.transform.SetPositionAndRotation(CrownSpawnLocation.position, CrownSpawnLocation.rotation);
                    
                    // Make sure it has no parent that might override its position
                    if (Crown.transform.parent != null)
                        Crown.transform.SetParent(null);
                    
                    // Now activate the crown
                    Crown.SetActive(true);
                    
                    // Play effects
                    if (CrownAppearEffect != null)
                    {
                        Instantiate(CrownAppearEffect, CrownSpawnLocation.position, Quaternion.identity);
                    }
                    
                    if (CrownAppearSound != null)
                    {
                        AudioUtility.CreateSFX(CrownAppearSound, CrownSpawnLocation.position, AudioUtility.AudioGroups.Pickup, 0f);
                    }
                    
                    // Display message
                    DisplayMessageEvent displayMessage = EventsGame.DisplayMessageEvent;
                    displayMessage.Message = "You found the Ancient Crown!";
                    displayMessage.DelayBeforeDisplay = 0f;
                    EventManager.Broadcast(displayMessage);
                    
                    // Notify the objectives system if needed
                    if (medievalObjectives != null)
                    {
                        // Call OnCrownRevealed instead of OnCrownFound
                        medievalObjectives.OnCrownRevealed();
                    }
                }
                
                // Disable this trigger after use
                gameObject.SetActive(false);
            }
        }
    }
}