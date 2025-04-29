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
        
        [Tooltip("Reference to MedievalObjectives component")]
        public MedievalObjectives MedievalObjectives;
        
        private bool hasTriggered = false;
        
        void Start()
        {
            // Get reference to MedievalObjectives if not set
            if (MedievalObjectives == null)
                MedievalObjectives = FindObjectOfType<MedievalObjectives>();
            
            // Make sure the crown is initially inactive
            if (Crown != null)
                Crown.SetActive(false);
                
            // Make sure this trigger is initially disabled
            // It will be enabled by MedievalObjectives when needed
            gameObject.SetActive(false);
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
                    // Set the crown's position and rotation
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
                    
                    // Display a message
                    DisplayMessageEvent displayMessage = EventsGame.DisplayMessageEvent;
                    displayMessage.Message = "You found the Ancient Crown!";
                    displayMessage.DelayBeforeDisplay = 0f;
                    EventManager.Broadcast(displayMessage);
                    
                    // Notify the MedievalObjectives
                    if (MedievalObjectives != null)
                    {
                        MedievalObjectives.OnCrownRevealed();
                    }
                    else
                    {
                        Debug.LogError("MedievalObjectives reference is missing!");
                    }
                }
                
                // Disable this trigger after use
                gameObject.SetActive(false);
            }
        }
    }
}