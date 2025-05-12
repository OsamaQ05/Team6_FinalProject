using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    public class SequentialObjective : MonoBehaviour
    {
        [System.Serializable]
        public class ObjectiveStep
        {
            [Tooltip("Title of this objective step")]
            public string Title;
            
            [Tooltip("Description of this objective step")]
            public string Description;
            
            [Tooltip("Is this objective optional?")]
            public bool IsOptional = false;
            
            [Tooltip("Counter text to show for this objective (can be empty)")]
            public string CounterText = "";
            
            [Tooltip("Notification to show when this objective becomes active")]
            public string ActivationNotification = "";
            
            [Tooltip("Notification to show when this objective is completed")]
            public string CompletionNotification = "";
            
            [HideInInspector]
            public bool IsActive = false;
            
            [HideInInspector]
            public bool IsCompleted = false;
        }
        
        [Tooltip("List of sequential objectives")]
        public List<ObjectiveStep> ObjectiveSteps = new List<ObjectiveStep>();
        
        [Tooltip("Delay between objectives in seconds")]
        public float DelayBetweenObjectives = 2f;
        
        [Tooltip("Delay before starting first objective")]
        public float StartDelay = 1f;
        
        private int currentObjectiveIndex = -1;
        private ActiveObjective currentActiveObjective;
        private bool initialObjectiveCreated = false;
        
        void Start()
        {
            // Only create the first objective after a delay to avoid duplicates
            if (!initialObjectiveCreated)
            {
                initialObjectiveCreated = true;
                Invoke("AdvanceToNextObjective", StartDelay);
            }
        }
        
        public void AdvanceToNextObjective()
        {
            // Complete current objective if there is one
            if (currentObjectiveIndex >= 0 && currentObjectiveIndex < ObjectiveSteps.Count)
            {
                ObjectiveStep currentStep = ObjectiveSteps[currentObjectiveIndex];
                if (currentStep.IsActive && !currentStep.IsCompleted)
                {
                    CompleteCurrentObjective(currentStep.CompletionNotification);
                }
            }
            
            // Move to next objective
            currentObjectiveIndex++;
            
            // Check if we have more objectives
            if (currentObjectiveIndex < ObjectiveSteps.Count)
            {
                // Create the next objective
                CreateNextObjective();
            }
            else
            {
                // All objectives completed
                Debug.Log("All sequential objectives completed!");
            }
        }
        
        public bool AreAllObjectivesCompleted()
        {
            return currentObjectiveIndex >= ObjectiveSteps.Count;
        }
        
        public int GetCurrentObjectiveIndex()
        {
            return currentObjectiveIndex;
        }
        
        private void CreateNextObjective()
        {
            // Safely clean up any existing objective component first
            if (currentActiveObjective != null)
            {
                Destroy(currentActiveObjective);
                currentActiveObjective = null;
            }
            
            ObjectiveStep step = ObjectiveSteps[currentObjectiveIndex];
            step.IsActive = true;
            
            // Display notification if one is set
            if (!string.IsNullOrEmpty(step.ActivationNotification))
            {
                DisplayMessage(step.ActivationNotification, 0f);
            }
            
            // Create an ActiveObjective component
            currentActiveObjective = gameObject.AddComponent<ActiveObjective>();
            
            // Set up the objective
            currentActiveObjective.SetupObjective(step.Title, step.Description, step.IsOptional);
            
            // Update counter text if provided
            if (!string.IsNullOrEmpty(step.CounterText))
            {
                currentActiveObjective.UpdateObjective("", step.CounterText, "");
            }
            
            Debug.Log("Created objective: " + step.Title);
        }
        
        public void CompleteCurrentObjective(string notificationText = "")
        {
            if (currentObjectiveIndex < 0 || currentObjectiveIndex >= ObjectiveSteps.Count)
                return;
                
            ObjectiveStep step = ObjectiveSteps[currentObjectiveIndex];
            step.IsCompleted = true;
            
            if (currentActiveObjective != null)
            {
                currentActiveObjective.CompleteObjective("", step.CounterText, 
                    string.IsNullOrEmpty(notificationText) ? "Objective complete: " + step.Title : notificationText);
                
                // Destroy the objective component after completion
                Destroy(currentActiveObjective);
                currentActiveObjective = null;
            }
        }
        
        // Replace only the DisplayMessage method in your SequentialObjective.cs file:

        private void DisplayMessage(string message, float delay)
        {
            // Make sure to use the fully qualified namespace
            Unity.FPS.Game.DisplayMessageEvent displayMessage = Unity.FPS.Game.EventsGame.DisplayMessageEvent;
            displayMessage.Message = message;
            displayMessage.DelayBeforeDisplay = delay;
            Unity.FPS.Game.EventManager.Broadcast(displayMessage);
        }
        
        // This ensures we don't duplicate objectives if the game is paused/resumed
        void OnEnable()
        {
            // Only advance if we've never created the first objective
            if (!initialObjectiveCreated && currentObjectiveIndex == -1)
            {
                initialObjectiveCreated = true;
                Invoke("AdvanceToNextObjective", StartDelay);
            }
        }
        
        void OnDisable()
        {
            CancelInvoke("AdvanceToNextObjective");
        }
    }
    
    // This class handles a single active objective in the sequence
    public class ActiveObjective : Objective
    {
        public void SetupObjective(string title, string description, bool isOptional)
        {
            Title = title;
            Description = description;
            IsOptional = isOptional;
            
            // Force a Start call to register with the objective system
            base.Start();
        }
        
        protected override void Start()
        {
            // Override to prevent duplicate Start call
            // The base.Start() will be called explicitly in SetupObjective
        }
    }
}