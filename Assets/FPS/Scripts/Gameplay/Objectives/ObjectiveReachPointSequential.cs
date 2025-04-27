using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class ObjectiveReachPointSequential : Objective
    {
        [Tooltip("Visible transform that will be destroyed once the objective is completed")]
        public Transform DestroyRoot;

        // Removed the NextObjective field as the SequentialObjectiveManager handles this

        void Awake()
        {
            // Keep Awake if DestroyRoot needs defaulting
            if (DestroyRoot == null)
                DestroyRoot = transform;

            // Deactivate the objective by default.
            // The SequentialObjectiveManager will activate it when it's time.
            gameObject.SetActive(false);
        }

        // Removed the Start method - SequentialObjectiveManager controls activation

        void OnTriggerEnter(Collider other)
        {
            // No need to check IsCompleted here if the object is only active when it's the current objective
            // However, checking IsCompleted is still good practice to prevent potential double triggers
            if (IsCompleted)
                 return;

            var player = other.GetComponent<PlayerCharacterController>();
            // Ensure the colliding object is a player
            if (player != null)
            {
                // Call the base Objective class's CompleteObjective method.
                // This will trigger the OnCompleted event, which the SequentialObjectiveManager listens for.
                CompleteObjective(string.Empty, string.Empty, "Objective complete: " + Title);

                // Optional: Destroy the visual part of the objective
                if (DestroyRoot != null)
                {
                    Destroy(DestroyRoot.gameObject);
                }

                // Do NOT activate the next objective here. The manager handles it.
                // The objective's GameObject might be deactivated by the manager shortly after completion anyway.
            }
        }
    }
}