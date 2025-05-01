using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(EgyptSequentialObjective))]
    public class EgyptCheckpointManager : MonoBehaviour
    {
        [System.Serializable]
        public class CheckpointInfo
        {
            [Tooltip("The checkpoint GameObject for this objective")]
            public GameObject CheckpointObject;
            
            [Tooltip("Title for this checkpoint objective")]
            public string Title = "Reach the Checkpoint";
            
            [Tooltip("Description for this checkpoint")]
            public string Description = "Navigate to the marked location";
            
            [Tooltip("Notification when this checkpoint becomes active")]
            public string ActivationMessage = "Find the next checkpoint";
            
            [Tooltip("Notification when this checkpoint is reached")]
            public string CompletionMessage = "Checkpoint reached!";
            
            [Tooltip("Optional marker or effect to show when this checkpoint is active")]
            public GameObject VisualIndicator;
            
            [Tooltip("Optional sound to play when this checkpoint becomes active")]
            public AudioClip ActivationSound;
            
            [Tooltip("Objects to reveal when this checkpoint becomes active")]
            public List<GameObject> ObjectsToReveal = new List<GameObject>();
            
            [Tooltip("Objects to hide when this checkpoint is completed")]
            public List<GameObject> ObjectsToHideAfterCompletion = new List<GameObject>();
            
            [HideInInspector]
            public bool IsCompleted = false;
        }
        
        [Header("Checkpoint Settings")]
        [Tooltip("List of checkpoints in sequence")]
        public List<CheckpointInfo> Checkpoints = new List<CheckpointInfo>();
        
        [Tooltip("Distance required to trigger checkpoint completion")]
        public float TriggerDistance = 3f;
        
        [Tooltip("Time to wait between revealing checkpoints")]
        public float DelayBetweenCheckpoints = 2f;
        
        [Header("Effects")]
        [Tooltip("Optional effect to play when checkpoint is completed")]
        public GameObject CheckpointCompletionEffect;
        
        [Tooltip("Audio to play when checkpoint is completed")]
        public AudioClip CheckpointCompletionSound;
        
        [Header("Final Reward")]
        [Tooltip("Object to activate after completing all checkpoints")]
        public GameObject FinalReward;
        
        [Tooltip("Additional objects to reveal after all checkpoints are completed")]
        public List<GameObject> FinalObjectsToReveal = new List<GameObject>();
        
        [Header("Scene Transition")]
        [Tooltip("Should the scene change after all checkpoints are reached?")]
        public bool ChangeSceneOnCompletion = false;
        
        [Tooltip("Name of the scene to load after completing all checkpoints")]
        public string WinSceneName = "WinScene";
        
        [Tooltip("Delay before loading the win scene (seconds)")]
        public float WinSceneLoadDelay = 3f;
        
        [Tooltip("Optional victory sound to play before scene transition")]
        public AudioClip VictorySound;
        
        private EgyptSequentialObjective sequentialObjective;
        private PlayerCharacterController playerController;
        private int currentCheckpointIndex = -1;
        private bool isTransitioningToWinScene = false;
        
        void Awake()
        {
            // Get reference to the EgyptSequentialObjective component
            sequentialObjective = GetComponent<EgyptSequentialObjective>();
            
            // Find the player controller
            playerController = FindFirstObjectByType<PlayerCharacterController>();
            
            // Make sure all checkpoints are initially hidden
            HideAllCheckpoints();
            
            // Hide all objects to reveal initially
            HideAllRevealObjects();
            
            // Hide final reward if assigned
            if (FinalReward != null)
                FinalReward.SetActive(false);
                
            // Hide all final objects to reveal
            foreach (var obj in FinalObjectsToReveal)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
        
        void Start()
        {
            // Initialize the sequential objectives based on checkpoints
            InitializeObjectives();
            
            // Connect to the objective changed event
            sequentialObjective.OnObjectiveChanged += HandleObjectiveChanged;
        }
        
        void OnDestroy()
        {
            // Clean up event subscription
            if (sequentialObjective != null)
                sequentialObjective.OnObjectiveChanged -= HandleObjectiveChanged;
        }
        
        void Update()
        {
            // Check if we need to detect player reaching current checkpoint
            if (currentCheckpointIndex >= 0 && currentCheckpointIndex < Checkpoints.Count)
            {
                CheckpointInfo currentCheckpoint = Checkpoints[currentCheckpointIndex];
                
                // Skip if already completed
                if (currentCheckpoint.IsCompleted)
                    return;
                
                // Check distance to current checkpoint
                if (playerController != null && currentCheckpoint.CheckpointObject != null)
                {
                    float distance = Vector3.Distance(
                        playerController.transform.position,
                        currentCheckpoint.CheckpointObject.transform.position
                    );
                    
                    // If player is close enough to checkpoint
                    if (distance <= TriggerDistance)
                    {
                        // Mark checkpoint as completed
                        currentCheckpoint.IsCompleted = true;
                        
                        // Play effects
                        PlayCheckpointCompletionEffects(currentCheckpoint.CheckpointObject.transform.position);
                        
                        // Hide objects that should be hidden after completion
                        foreach (var obj in currentCheckpoint.ObjectsToHideAfterCompletion)
                        {
                            if (obj != null)
                                obj.SetActive(false);
                        }
                        
                        // Advance to next objective
                        sequentialObjective.AdvanceToNextObjective();
                        
                        // The checkpoint visibility will be updated via the event callback
                    }
                }
            }
        }
        
        // Event handler for objective changes
        private void HandleObjectiveChanged(int newObjectiveIndex)
        {
            currentCheckpointIndex = newObjectiveIndex;
            UpdateCheckpointVisibility();
            
            // Check if all checkpoints are completed and we should transition to win scene
            if (currentCheckpointIndex >= Checkpoints.Count && ChangeSceneOnCompletion && !isTransitioningToWinScene)
            {
                StartCoroutine(TransitionToWinScene());
            }
        }
        
        private IEnumerator TransitionToWinScene()
        {
            isTransitioningToWinScene = true;
            
            // Play victory sound if assigned
            if (VictorySound != null)
            {
                AudioUtility.CreateSFX(
                    VictorySound,
                    Camera.main.transform.position,
                    AudioUtility.AudioGroups.HUDVictory,
                    0f
                );
            }
            
            // Display a completion message
            DisplayMessageEvent displayMessage = EventsGame.DisplayMessageEvent;
            displayMessage.Message = "All objectives completed!";
            displayMessage.DelayBeforeDisplay = 0f;
            EventManager.Broadcast(displayMessage);
            
            // Wait for the specified delay
            yield return new WaitForSeconds(WinSceneLoadDelay);
            
            // Unlock cursor for the win scene
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Load the win scene
            Debug.Log("Loading win scene: " + WinSceneName);
            SceneManager.LoadScene(WinSceneName);
        }
        
        private void InitializeObjectives()
        {
            // Clear any existing objectives
            sequentialObjective.ObjectiveSteps.Clear();
            
            // Add an objective step for each checkpoint
            foreach (var checkpoint in Checkpoints)
            {
                sequentialObjective.ObjectiveSteps.Add(new EgyptSequentialObjective.ObjectiveStep
                {
                    Title = checkpoint.Title,
                    Description = checkpoint.Description,
                    IsOptional = false,
                    ActivationNotification = checkpoint.ActivationMessage,
                    CompletionNotification = checkpoint.CompletionMessage
                });
            }
            
            // Add final objective if needed (e.g., "Return to entrance")
            if (FinalReward != null || FinalObjectsToReveal.Count > 0 || ChangeSceneOnCompletion)
            {
                sequentialObjective.ObjectiveSteps.Add(new EgyptSequentialObjective.ObjectiveStep
                {
                    Title = "Mission Complete",
                    Description = "You've completed all objectives",
                    IsOptional = false,
                    ActivationNotification = "All checkpoints completed!",
                    CompletionNotification = "Mission accomplished!"
                });
            }
        }
        
        private void HideAllCheckpoints()
        {
            foreach (var checkpoint in Checkpoints)
            {
                if (checkpoint.CheckpointObject != null)
                {
                    // Disable the entire GameObject to truly hide it
                    checkpoint.CheckpointObject.SetActive(false);
                    
                    // Disable any visual indicators
                    if (checkpoint.VisualIndicator != null)
                        checkpoint.VisualIndicator.SetActive(false);
                }
            }
        }
        
        private void HideAllRevealObjects()
        {
            // Hide all objects that should be revealed later
            foreach (var checkpoint in Checkpoints)
            {
                foreach (var obj in checkpoint.ObjectsToReveal)
                {
                    if (obj != null)
                        obj.SetActive(false);
                }
            }
        }
        
        private void UpdateCheckpointVisibility()
        {
            // First hide all checkpoints
            HideAllCheckpoints();
            
            // Then show only the current one
            if (currentCheckpointIndex >= 0 && currentCheckpointIndex < Checkpoints.Count)
            {
                CheckpointInfo currentCheckpoint = Checkpoints[currentCheckpointIndex];
                
                if (currentCheckpoint.CheckpointObject != null)
                {
                    // Enable the checkpoint GameObject
                    currentCheckpoint.CheckpointObject.SetActive(true);
                    
                    // Enable visual indicator if assigned
                    if (currentCheckpoint.VisualIndicator != null)
                        currentCheckpoint.VisualIndicator.SetActive(true);
                    
                    // Reveal objects associated with this checkpoint
                    foreach (var obj in currentCheckpoint.ObjectsToReveal)
                    {
                        if (obj != null)
                        {
                            obj.SetActive(true);
                            Debug.Log("Revealing object: " + obj.name);
                        }
                    }
                    
                    // Play activation sound if assigned
                    if (currentCheckpoint.ActivationSound != null)
                    {
                        AudioUtility.CreateSFX(
                            currentCheckpoint.ActivationSound, 
                            currentCheckpoint.CheckpointObject.transform.position, 
                            AudioUtility.AudioGroups.Pickup, 
                            0f
                        );
                    }
                    
                    Debug.Log("Showing checkpoint: " + currentCheckpoint.Title);
                }
            }
            else if (currentCheckpointIndex >= Checkpoints.Count)
            {
                // All checkpoints completed
                
                // Show final reward if assigned
                if (FinalReward != null)
                {
                    FinalReward.SetActive(true);
                    Debug.Log("All checkpoints completed - showing final reward");
                }
                
                // Reveal final objects
                foreach (var obj in FinalObjectsToReveal)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                        Debug.Log("Revealing final object: " + obj.name);
                    }
                }
            }
        }
        
        private void PlayCheckpointCompletionEffects(Vector3 position)
        {
            // Play effect if assigned
            if (CheckpointCompletionEffect != null)
            {
                Instantiate(CheckpointCompletionEffect, position, Quaternion.identity);
            }
            
            // Play sound if assigned
            if (CheckpointCompletionSound != null)
            {
                AudioUtility.CreateSFX(
                    CheckpointCompletionSound, 
                    position, 
                    AudioUtility.AudioGroups.Pickup, 
                    0f
                );
            }
        }
        
        private void DisplayMessage(string message, float delay)
        {
            // Make sure to use the fully qualified namespace
            Unity.FPS.Game.DisplayMessageEvent displayMessage = Unity.FPS.Game.EventsGame.DisplayMessageEvent;
            displayMessage.Message = message;
            displayMessage.DelayBeforeDisplay = delay;
            Unity.FPS.Game.EventManager.Broadcast(displayMessage);
        }
    }
}