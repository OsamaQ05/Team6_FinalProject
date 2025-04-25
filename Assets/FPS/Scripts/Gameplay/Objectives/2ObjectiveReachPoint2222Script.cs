using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class ObjectiveReachPoint2 : Objective
    {
        [Tooltip("Name of the scene to load upon reaching the objective")]
        public string NextSceneName;

        void OnTriggerEnter(Collider other)
        {
            if (IsCompleted)
                return;

            var player = other.GetComponent<PlayerCharacterController>();
            if (player != null)
            {
                CompleteObjective(string.Empty, string.Empty, "Objective complete : " + Title);

                if (!string.IsNullOrEmpty(NextSceneName))
                {
                    SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene load event
                    SceneManager.LoadScene(NextSceneName);
                    SceneManager.UnloadSceneAsync("MainScene");
                }
                else
                {
                    Debug.LogWarning("NextSceneName is not set!");
                }
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe after executing

            GameObject player = GameObject.FindWithTag("Play"); // Find the player
            GameObject spawnPoint = GameObject.FindWithTag("Spawn Point"); // Find the spawn point

            if (player != null && spawnPoint != null)
            {
                player.transform.position = spawnPoint.transform.position;  // Move player to spawn
                player.transform.rotation = spawnPoint.transform.rotation;
            }
            else
            {
                Debug.LogWarning("Player or SpawnPoint not found in scene!");
            }
        }
    }
}
