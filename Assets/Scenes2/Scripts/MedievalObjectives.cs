using UnityEngine;
using Unity.FPS.Game;
using System.Collections;
using System;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(SequentialObjective))]
    public class MedievalObjectives : MonoBehaviour
    {
        [Header("Game References")]
        [Tooltip("Reference to the Medieval Manager")]
        public MedievalManager MedievalManager;
        
        [Header("Crown Objective Settings")]
        [Tooltip("The GameObject containing the crown that must be returned")]
        public GameObject Crown;
        
        [Tooltip("The location where the crown must be returned")]
        public Transform CrownReturnLocation;
        
        [Tooltip("The distance within which the crown is considered returned")]
        public float ReturnDistance = 2f;
        
        [Tooltip("GameObject that will be enabled when crown is returned")]
        public GameObject CrownReturnEffect;
        
        [Tooltip("Audio clip to play when crown is returned")]
        public AudioClip CrownReturnSound;
        
        [Header("Crown Spawn Point")]
        [Tooltip("Trigger area that reveals the crown location")]
        public GameObject CrownSpawnPointTrigger;
        
        [Header("Optional Kill Objective")]
        [Tooltip("Whether the kill objective is enabled")]
        public bool EnableKillObjective = true;
        
        [Tooltip("Number of enemies to kill")]
        public int EnemiesToKill = 5;
        
        private SequentialObjective sequentialObjective;
        private bool isCrownObjectiveActive = false;
        private bool isPlayerInReturnZone = false;
        private PlayerCharacterController playerCharacterController;
        private int killedEnemies = 0;
        private int lastKnownStatueCount = 0;
        
        // Add this to prevent the crown from having pickup behavior
        // Don't want the crown to disappear on touch
        private bool isPickupDisabled = false;
        
        void Awake()
        {
            sequentialObjective = GetComponent<SequentialObjective>();
            
            // Initialize sequential objectives
            sequentialObjective.ObjectiveSteps.Clear();
            
            // Add torch lighting objective
            sequentialObjective.ObjectiveSteps.Add(new SequentialObjective.ObjectiveStep
            {
                Title = "Light the Sacred Torches",
                Description = "Find and light all the sacred torches",
                IsOptional = false,
                CounterText = "0 / " + MedievalManager.TotalStatues,
                ActivationNotification = "",
                CompletionNotification = "Torches Completed!"
            });
            
            // Add crown return objective
            sequentialObjective.ObjectiveSteps.Add(new SequentialObjective.ObjectiveStep
            {
                Title = "Return the Crown",
                Description = "Return the ancient crown to the altar",
                IsOptional = false,
                ActivationNotification = "",
                CompletionNotification = "Crown Returned!"
            });
            
            // Add artifact (mask) retrieval objective
            sequentialObjective.ObjectiveSteps.Add(new SequentialObjective.ObjectiveStep
            {
                Title = "Retrieve the Sacred Mask",
                Description = "The ancient mask has appeared in the ritual chamber. Find and collect it to complete the ceremony.",
                IsOptional = false,
                ActivationNotification = "",
                CompletionNotification = ""
            });
            
            // Add optional kill objective if enabled
            if (EnableKillObjective)
            {
                sequentialObjective.ObjectiveSteps.Add(new SequentialObjective.ObjectiveStep
                {
                    Title = "Defeat Enemies",
                    Description = "Defeat the evil spirits",
                    IsOptional = true,
                    CounterText = "0 / " + EnemiesToKill,
                    ActivationNotification = "Optional: Defeat the evil spirits",
                });
            }
            
            // Listen to enemy kill events
            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);
        }
        
        void Start()
        {
            // Find player
            playerCharacterController = FindObjectOfType<PlayerCharacterController>();
            
            // Hide the crown initially
            if (Crown != null)
            {
                Crown.SetActive(false);
                
                // IMPORTANT: Disable any Pickup component on the crown so it doesn't vanish when touched
                DisableCrownPickup();
            }
                
            // Make sure the crown spawn point trigger is disabled initially
            if (CrownSpawnPointTrigger != null)
                CrownSpawnPointTrigger.SetActive(false);
                
            // Start tracking torch lighting progress
            StartCoroutine(MonitorObjectives());
        }
        
        // Add this method to disable any Pickup component on the Crown
        void DisableCrownPickup()
        {
            if (Crown != null && !isPickupDisabled)
            {
                // Check for Pickup component and disable it
                Pickup pickup = Crown.GetComponent<Pickup>();
                if (pickup != null)
                {
                    pickup.enabled = false;
                    Debug.Log("Disabled Pickup component on Crown");
                }
                
                // Check for any collider component that might be set as a trigger
                Collider crowncollider = Crown.GetComponent<Collider>();
                if (crowncollider != null && crowncollider.isTrigger)
                {
                    // Either disable the trigger or make it non-trigger
                    // crowncollider.isTrigger = false;
                    Debug.Log("Modified collider on Crown");
                }
                
                isPickupDisabled = true;
            }
        }
        
        IEnumerator AdvanceToCrownReturnObjective(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // Complete the torch objective
            sequentialObjective.AdvanceToNextObjective();
            
            // Activate the crown spawn trigger
            if (CrownSpawnPointTrigger != null)
            {
                Debug.Log("Activating crown spawn trigger");
                CrownSpawnPointTrigger.SetActive(true);
            }
            else
            {
                Debug.LogError("Crown spawn trigger is not assigned!");
                // Fallback - directly activate the crown
                if (Crown != null)
                {
                    Crown.SetActive(true);
                    isCrownObjectiveActive = true;
                }
            }
        }

        // This gets called from the CrownSpawnPoint script
        public void OnCrownRevealed()
        {
            Debug.Log("Crown revealed by spawn trigger");
            
            // Make sure the crown can't be picked up (only returned)
            DisableCrownPickup();
            
            // Set the crown objective as active
            isCrownObjectiveActive = true;
            
            // If the current objective index is already at 1, we don't need to do anything else
            // The return objective should already be active
        }
        
        void Update()
        {
            // Check if the statue count has changed (fallback monitoring method)
            if (MedievalManager.statuesCleansed != lastKnownStatueCount)
            {
                lastKnownStatueCount = MedievalManager.statuesCleansed;
                
                // This monitors statue cleansing without relying on events
                if (sequentialObjective.GetCurrentObjectiveIndex() == 0)
                {
                    UpdateTorchCounter(lastKnownStatueCount);
                    
                    // Check if all torches are lit
                    if (MedievalManager.AreAllStatuesCleansed())
                    {
                        StartCoroutine(AdvanceToCrownReturnObjective(1f));
                    }
                }
            }
            
            // Check if crown objective is active
            if (isCrownObjectiveActive && 
                sequentialObjective.GetCurrentObjectiveIndex() == 1 && // Now index 1 is the return crown objective
                CrownReturnLocation != null &&
                playerCharacterController != null &&
                Crown != null && Crown.activeSelf) // Make sure crown is actually active
            {
                // Check if player is in crown return zone
                float distance = Vector3.Distance(playerCharacterController.transform.position, 
                                                CrownReturnLocation.position);
                                                
                if (distance <= ReturnDistance)
                {
                    if (!isPlayerInReturnZone)
                    {
                        isPlayerInReturnZone = true;
                        ReturnCrown();
                    }
                }
                else
                {
                    isPlayerInReturnZone = false;
                }
            }
        }
        
        IEnumerator MonitorObjectives()
        {
            while (true)
            {
                // This is now just a fallback polling mechanism
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        void UpdateTorchCounter(int currentStatues)
        {
            if (sequentialObjective.GetCurrentObjectiveIndex() == 0)
            {
                string counterText = currentStatues + " / " + MedievalManager.TotalStatues;
                Debug.Log("Updating torch counter: " + counterText);
                
                // Get the current active objective component - might be null during initialization
                ActiveObjective activeObjective = GetComponent<ActiveObjective>();
                if (activeObjective != null)
                {
                    activeObjective.UpdateObjective("", counterText, "");
                }
            }
        }
        
        void ReturnCrown()
        {
            // Play effects
            if (CrownReturnEffect != null)
                CrownReturnEffect.SetActive(true);
                
            if (CrownReturnSound != null)
                AudioUtility.CreateSFX(CrownReturnSound, transform.position, AudioUtility.AudioGroups.Pickup, 0f);
                
            // Hide crown
            if (Crown != null)
                Crown.SetActive(false);
                
            // Complete objective and move to next
            sequentialObjective.AdvanceToNextObjective();
            isCrownObjectiveActive = false;
            
            // Trigger mask spawn in the medieval manager
            StartCoroutine(DelayedMaskSpawn());
        }
        
        IEnumerator DelayedMaskSpawn()
        {
            yield return new WaitForSeconds(MedievalManager.ArtifactSpawnDelay);
            
            // Spawn the mask artifact
            MedievalManager.SpawnArtifact();
            
            // Monitor for artifact collection
            StartCoroutine(MonitorArtifactCollection());
        }
        
        IEnumerator MonitorArtifactCollection()
        {
            bool artifactWasSpawned = MedievalManager.artifactSpawned;
            
            while (artifactWasSpawned)
            {
                // Check if the artifact is still spawned
                if (!MedievalManager.artifactSpawned)
                {
                    // Artifact was collected, complete the objective
                    sequentialObjective.AdvanceToNextObjective();
                    break;
                }
                
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (EnableKillObjective)
            {
                killedEnemies++;
                
                // Find the kill objective index (should be the last one)
                int killObjectiveIndex = EnableKillObjective ? 3 : -1; // Updated index for correct step
                
                // Update kill counter if kill objective is active
                if (sequentialObjective.GetCurrentObjectiveIndex() == killObjectiveIndex)
                {
                    ActiveObjective activeObjective = GetComponent<ActiveObjective>();
                    if (activeObjective != null)
                    {
                        string counterText = killedEnemies + " / " + EnemiesToKill;
                        activeObjective.UpdateObjective("", counterText, "");
                        
                        // Check if kill objective is complete
                        if (killedEnemies >= EnemiesToKill)
                        {
                            sequentialObjective.CompleteCurrentObjective("All enemies defeated!");
                        }
                    }
                }
            }
        }
        
        void OnDestroy()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
        }
    }
}