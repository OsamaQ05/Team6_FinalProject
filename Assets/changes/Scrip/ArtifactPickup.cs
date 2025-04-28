using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ModularArtifactPickup : Pickup
    {
        [Header("Artifact Settings")]
        [Tooltip("Additional VFX to play when artifact is collected")]
        public GameObject SpecialCollectionVfx;
        
        [Tooltip("Special audio for artifact collection")]
        public AudioClip ArtifactCollectionSfx;
        
        [Tooltip("Check this if this artifact belongs to the Medieval scene")]
        public bool UseMedievalManager = false;
        
        [Tooltip("Check this if this artifact belongs to the Samurai scene")]
        public bool UseSamuraiManager = false;

        protected override void Start()
        {
            base.Start();
            
            // Try to find the appropriate manager
            if (UseMedievalManager)
            {
                MedievalManager medievalManager = FindObjectOfType<MedievalManager>();
                if (medievalManager != null && !medievalManager.AreAllStatuesCleansed())
                {
                    gameObject.SetActive(false);
                }
            }
            else if (UseSamuraiManager)
            {
                SamuraiGameManager samuraiManager = FindObjectOfType<SamuraiGameManager>();
                if (samuraiManager != null && !samuraiManager.AreAllStatuesCleansed())
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                // If no specific manager is selected, try to find either one
                MedievalManager medievalManager = FindObjectOfType<MedievalManager>();
                if (medievalManager != null)
                {
                    UseMedievalManager = true;
                }
                else
                {
                    SamuraiGameManager samuraiManager = FindObjectOfType<SamuraiGameManager>();
                    if (samuraiManager != null)
                    {
                        UseSamuraiManager = true;
                    }
                }
            }
        }

        protected override void OnPicked(PlayerCharacterController playerController)
        {
            // Play the base pickup feedback (sound, VFX)
            base.OnPicked(playerController);
            
            // Play special artifact collection effects if assigned
            if (SpecialCollectionVfx != null)
            {
                Instantiate(SpecialCollectionVfx, transform.position, Quaternion.identity);
            }
            
            if (ArtifactCollectionSfx != null)
            {
                AudioUtility.CreateSFX(ArtifactCollectionSfx, transform.position, AudioUtility.AudioGroups.Pickup, 0f);
            }

            // Notify the appropriate game manager that the artifact was collected
            if (UseMedievalManager)
            {
                MedievalManager medievalManager = FindObjectOfType<MedievalManager>();
                if (medievalManager != null)
                {
                    Debug.Log("Artifact collected - notifying MedievalManager");
                    medievalManager.OnArtifactCollected();
                }
                else
                {
                    Debug.LogError("MedievalManager not found!");
                }
            }
            else if (UseSamuraiManager)
            {
                SamuraiGameManager samuraiManager = FindObjectOfType<SamuraiGameManager>();
                if (samuraiManager != null)
                {
                    Debug.Log("Artifact collected - notifying SamuraiGameManager");
                    samuraiManager.OnArtifactCollected();
                }
                else
                {
                    Debug.LogError("SamuraiGameManager not found!");
                }
            }
            else
            {
                Debug.LogError("No game manager type selected or detected for artifact pickup!");
            }
            
            // Destroy the artifact object after pickup
            Destroy(gameObject);
        }
    }
}