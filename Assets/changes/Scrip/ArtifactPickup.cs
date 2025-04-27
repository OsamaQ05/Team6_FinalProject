using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ArtifactPickup : Pickup
    {
        [Header("Artifact Settings")]
        [Tooltip("Additional VFX to play when artifact is collected")]
        public GameObject SpecialCollectionVfx;
        
        [Tooltip("Special audio for artifact collection")]
        public AudioClip ArtifactCollectionSfx;

        protected override void Start()
        {
            base.Start();
            
            // You can add any specific initialization for the artifact here
            // For example, initially hiding it until statues are cleansed
            if (SamuraiGameManager.Instance != null && !SamuraiGameManager.Instance.AreAllStatuesCleansed())
            {
                gameObject.SetActive(false);
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

            // Notify the game manager that the artifact was collected
            if (SamuraiGameManager.Instance != null)
            {
                SamuraiGameManager.Instance.OnArtifactCollected();
            }
            
            // Destroy the artifact object after pickup
            Destroy(gameObject);
        }
    }
}