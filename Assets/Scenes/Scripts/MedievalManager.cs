using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Unity.FPS.Game
{
    public class MedievalManager : MonoBehaviour
    {
        [Header("General Settings")]
        public int TotalStatues = 3;
        public float EndSceneLoadDelay = 3f;
        public CanvasGroup EndGameFadeCanvasGroup;
        public string WinSceneName = "WinScene";
        public AudioClip VictorySound;

        [Header("Artifact Spawn Settings")]
        public float ArtifactSpawnDelay = 7f;
        public GameObject ArtifactPrefab; // Sacred Mask
        public Transform ArtifactSpawnPoint;

        [Header("Messages")]
        public string PlayerDeathMessage = "You have fallen... Try again!";

        // Making these public so they can be accessed by MedievalObjectives
        [HideInInspector] public int statuesCleansed = 0;
        [HideInInspector] public bool gameIsEnding = false;
        [HideInInspector] public bool artifactSpawned = false;
        
        float timeLoadEndScene;
        public static MedievalManager Instance;
        private GameObject spawnedArtifact;

        void Awake()
        {
            Instance = this;
            EventManager.AddListener<PlayerDeathEvent>(OnPlayerDeath);
        }

        void Start()
        {
            AudioUtility.SetMasterVolume(1);
            
            // No start message here anymore - handled by SequentialObjectives
                
            // Make sure artifact is not active at start
            if (ArtifactPrefab != null)
            {
                ArtifactPrefab.SetActive(false);
            }
        }

        void Update()
        {
            if (gameIsEnding)
            {
                float timeRatio = 1 - (timeLoadEndScene - Time.time) / EndSceneLoadDelay;
                EndGameFadeCanvasGroup.alpha = timeRatio;
                AudioUtility.SetMasterVolume(1 - timeRatio);

                if (Time.time >= timeLoadEndScene)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Restart if lost
                    gameIsEnding = false;
                }
            }
        }

        public void OnStatueCleansed(int statueNumber)
        {
            statuesCleansed++;
            
            // No message display here - handled by SequentialObjectives
        }

        // Public method to check if all statues are cleansed
        public bool AreAllStatuesCleansed()
        {
            return statuesCleansed >= TotalStatues;
        }

        // Made public so it can be called by MedievalObjectives
        public void SpawnArtifact()
        {
            if (ArtifactPrefab != null && ArtifactSpawnPoint != null && !artifactSpawned)
            {
                // Instantiate the artifact at the spawn point
                spawnedArtifact = Instantiate(ArtifactPrefab, ArtifactSpawnPoint.position, ArtifactSpawnPoint.rotation);
                spawnedArtifact.SetActive(true);
                artifactSpawned = true;
            }
        }

        public void OnArtifactCollected()
        {
            // Set the artifact as no longer spawned
            artifactSpawned = false;
            
            // Unlock cursor for the win scene
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Play victory sound
            if (VictorySound != null)
            {
                var audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = VictorySound;
                audioSource.playOnAwake = false;
                audioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDVictory);
                audioSource.Play();
            }

            // Load the win scene
            SceneManager.LoadScene(WinSceneName);
        }

        void OnPlayerDeath(PlayerDeathEvent evt)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            DisplayMessage(PlayerDeathMessage, 2f);

            gameIsEnding = true;
            EndGameFadeCanvasGroup.gameObject.SetActive(true);
            timeLoadEndScene = Time.time + EndSceneLoadDelay;
        }

        public void DisplayMessage(string message, float delayBeforeDisplay)
        {
            DisplayMessageEvent displayMessage = new DisplayMessageEvent();
            displayMessage.Message = message;
            displayMessage.DelayBeforeDisplay = delayBeforeDisplay;
            EventManager.Broadcast(displayMessage);
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<PlayerDeathEvent>(OnPlayerDeath);
        }
    }
}