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
            playerCharacterController = FindFirstObjectByType<PlayerCharacterController>();
            
            // Hide the crown initially
            if (Crown != null)
            {
                Crown.SetActive(false);
                
                // CRITICAL: Disable any Pickup component on the crown
                DisableCrownPickup();
            }
                
            // Make sure the crown spawn point trigger is disabled initially
            if (CrownSpawnPointTrigger != null)
                CrownSpawnPointTrigger.SetActive(false);
                
            // Start tracking torch lighting progress
            StartCoroutine(MonitorObjectives());
        }
        
        // Disable any Pickup component to prevent the crown from disappearing
        void DisableCrownPickup()
        {
            if (Crown != null && !isPickupDisabled)
            {
                // Check for ANY component that might be making the crown disappear
                // First, try standard Pickup component
                Pickup pickup = Crown.GetComponent<Pickup>();
                if (pickup != null)
                {
                    pickup.enabled = false;
                    Debug.Log("Disabled Pickup component on Crown");
                }
                
                // Check for any collider that might be set as a trigger
                Collider[] crowncolliders = Crown.GetComponentsInChildren<Collider>(true);
                foreach (Collider col in crowncolliders)
                {
                    if (col.isTrigger)
                    {
                        col.enabled = false;
                        Debug.Log("Disabled trigger collider on Crown or child");
                    }
                }
                
                // Check for any script with "pickup" in the name
                MonoBehaviour[] scripts = Crown.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour script in scripts)
                {
                    string scriptName = script.GetType().Name.ToLower();
                    if (scriptName.Contains("pickup") || scriptName.Contains("collect"))
                    {
                        script.enabled = false;
                        Debug.Log("Disabled possible pickup script: " + script.GetType().Name);
                    }
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
            Debug.Log("OnCrownRevealed called - Crown revealed by spawn trigger");
            
            // CRITICAL: Make sure ANY pickup behavior is disabled
            DisableCrownPickup();
            
            // Set the crown objective as active
            isCrownObjectiveActive = true;
            
            if (Crown != null)
            {
                Debug.Log("Crown active state: " + Crown.activeSelf);
            }
            
            Debug.Log("Current objective index: " + sequentialObjective.GetCurrentObjectiveIndex());
        }
        
        // NEW METHOD: Called by CrownSpawnTrigger to immediately advance to next objective
        public void AdvanceToNextObjectiveAfterCrownRevealed()
        {
            Debug.Log("Crown revealed, advancing to next objective");
            
            // Advance to the next objective
            sequentialObjective.AdvanceToNextObjective();
            
            // Trigger mask spawn
            StartCoroutine(DelayedMaskSpawn());
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
                playerCharacterController != null)
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
            Debug.Log("ReturnCrown called");
            
            // Play effects
            if (CrownReturnEffect != null)
                CrownReturnEffect.SetActive(true);
                
            if (CrownReturnSound != null)
                AudioUtility.CreateSFX(CrownReturnSound, transform.position, AudioUtility.AudioGroups.Pickup, 0f);
                
            // CRITICAL CHANGE: Do NOT hide the crown after return
            // The crown should remain visible where it was
            // if (Crown != null)
            //     Crown.SetActive(false);
                
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