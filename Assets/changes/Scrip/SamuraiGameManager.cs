using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Unity.FPS.Game
{
    public class SamuraiGameManager : MonoBehaviour
    {
        [Header("General Settings")]
        public int TotalStatues = 3;
        public float EndSceneLoadDelay = 3f;
        public CanvasGroup EndGameFadeCanvasGroup;
        public string WinSceneName = "WinScene";
        public AudioClip VictorySound;

        [Header("Enemy Spawning Settings")]
        public GameObject HoverBotPrefab;
        public List<Transform> Statue1SpawnPoints;
        public List<Transform> Statue2SpawnPoints;
        public List<Transform> Statue3SpawnPoints;
        public int EnemiesPerStatue = 3;
        public float SpawnDelay = 2f;

        [Header("Artifact Spawn Settings")]
        public float ArtifactSpawnDelay = 7f;
        public List<ParticleSystem> RiverParticles;
        public AudioSource RiverAudio;
        public GameObject ArtifactPrefab; // Imperial Regalia
        public Transform ArtifactSpawnPoint;

        [Header("Messages")]
        public string StartGameMessage = "Find and Cleanse the Three Sacred Statues!";
        public string AllStatuesCleansedMessage = "You Restored the Bushidō!";
        public string artifactMessage= " Now retrieve the Sacred Artifact!";
        public string PlayerDeathMessage = "You have fallen... Try again!";

        int statuesCleansed = 0;
        bool gameIsEnding = false;
        bool artifactSpawned = false;
        float timeLoadEndScene;
        public static SamuraiGameManager Instance;
        private GameObject spawnedArtifact;

        void Awake()
        {
            Instance = this;
            EventManager.AddListener<PlayerDeathEvent>(OnPlayerDeath);
        }

        void Start()
        {
            AudioUtility.SetMasterVolume(1);
            
            // 🔥 Show start message
            DisplayMessage(StartGameMessage, 2f);

            // 🔥 Stop river particles initially
            if (RiverParticles != null)
            {
                foreach (var particle in RiverParticles)
                {
                    if (particle != null)
                        particle.Stop();
                }
            }

            if (RiverAudio != null)
                RiverAudio.Stop();
                
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

            if (statuesCleansed >= TotalStatues)
            {
                StartCoroutine(HandleAllStatuesCleansed());
            }
            else
            {
                StartCoroutine(SpawnEnemiesDelayed(statueNumber));
            }
        }

        // Public method to check if all statues are cleansed
        public bool AreAllStatuesCleansed()
        {
            return statuesCleansed >= TotalStatues;
        }

        IEnumerator HandleAllStatuesCleansed()
        {
            // 🎯 Show win message immediately
            DisplayMessage(AllStatuesCleansedMessage, 2f);

            yield return new WaitForSeconds(ArtifactSpawnDelay);
            
            DisplayMessage(artifactMessage, 2f);

            // 🌊 Activate river particles
            if (RiverParticles != null)
            {
                foreach (var particle in RiverParticles)
                {
                    if (particle != null)
                        particle.Play();
                }
            }

            // 🎵 Play river sound
            if (RiverAudio != null)
                RiverAudio.Play();

            // 🗡️ Spawn artifact
            SpawnArtifact();
        }

        void SpawnArtifact()
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

        void SpawnEnemies(int statueNumber)
        {
            List<Transform> spawnList = GetSpawnPointsForStatue(statueNumber);

            if (HoverBotPrefab == null || spawnList.Count == 0)
                return;

            for (int i = 0; i < EnemiesPerStatue; i++)
            {
                Transform spawnPoint = spawnList[Random.Range(0, spawnList.Count)];
                Instantiate(HoverBotPrefab, spawnPoint.position, spawnPoint.rotation);
            }
        }

        List<Transform> GetSpawnPointsForStatue(int statueNumber)
        {
            switch (statueNumber)
            {
                case 1: return Statue1SpawnPoints;
                case 2: return Statue2SpawnPoints;
                case 3: return Statue3SpawnPoints;
                default: return new List<Transform>();
            }
        }

        IEnumerator SpawnEnemiesDelayed(int statueNumber)
        {
            yield return new WaitForSeconds(SpawnDelay);
            SpawnEnemies(statueNumber);
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
        // Replace only the DisplayMessage method in your SamuraiGameManager.cs file:

        void DisplayMessage(string message, float delayBeforeDisplay)
        {
            // Make sure to use the fully qualified namespace
            Unity.FPS.Game.DisplayMessageEvent displayMessage = Unity.FPS.Game.EventsGame.DisplayMessageEvent;
            displayMessage.Message = message;
            displayMessage.DelayBeforeDisplay = delayBeforeDisplay;
            Unity.FPS.Game.EventManager.Broadcast(displayMessage);
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<PlayerDeathEvent>(OnPlayerDeath);
        }
    }
}