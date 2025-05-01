using UnityEngine;
using Unity.FPS.Game;
using UnityEngine.UI;
using TMPro;
using Unity.FPS.Gameplay;

public class DeathQuizManager : MonoBehaviour
{
    [SerializeField] public GameObject quizGamePanel;
    [SerializeField] public QuestionGeneratorUI questionGeneratorUI;
    [SerializeField] private int requiredCorrectAnswers = 3; // Number of correct answers needed to pass
    [SerializeField] public Button tryAgainButton;
    [SerializeField] public TextMeshProUGUI resultText;
    [SerializeField] private float respawnDelay = 3f; // Time to wait before respawning after quiz

    private int currentCorrectAnswers = 0;
    private bool quizCompleted = false;
    private bool quizPassed = false;
    private bool isHandlingDeath = false;
    private bool isQuizActive = false;
    private float respawnTime;

    private void Awake()
    {
        // Make sure the quiz panel is hidden at start
        if (quizGamePanel != null)
            quizGamePanel.SetActive(false);
    }

    private void OnEnable()
    {
        // Subscribe to the player death event
        EventManager.AddListener<PlayerDeathEvent>(OnPlayerDeath);
    }

    private void OnDisable()
    {
        // Unsubscribe from the player death event
        EventManager.RemoveListener<PlayerDeathEvent>(OnPlayerDeath);
    }

    private void Update()
    {
        if (isQuizActive && quizCompleted && Time.time >= respawnTime)
        {
            RespawnPlayer();
        }
    }

    private void OnPlayerDeath(PlayerDeathEvent evt)
    {
        if (isHandlingDeath || isQuizActive) return;
        isHandlingDeath = true;
        isQuizActive = true;

        // Reset quiz state
        currentCorrectAnswers = 0;
        quizCompleted = false;
        quizPassed = false;

        // Show the quiz panel and generate questions
        if (quizGamePanel != null)
        {
            quizGamePanel.SetActive(true);
            
            // If we have the question generator, automatically start generating questions
            if (questionGeneratorUI != null)
            {
                questionGeneratorUI.GenerateQuestions();
            }
        }
    }

    // Call this method when an answer is submitted
    public void OnAnswerSubmitted(bool isCorrect)
    {
        if (quizCompleted) return;

        if (isCorrect)
        {
            currentCorrectAnswers++;
        }

        // Check if we have enough correct answers to pass
        if (currentCorrectAnswers >= requiredCorrectAnswers)
        {
            quizPassed = true;
            quizCompleted = true;
            ShowResult();
            // Set the respawn time
            respawnTime = Time.time + respawnDelay;
        }
    }

    private void ShowResult()
    {
        if (resultText != null)
        {
            if (quizPassed)
            {
                resultText.text = $"Congratulations! You passed the quiz. Respawning in {respawnDelay} seconds...";
                if (tryAgainButton != null)
                {
                    tryAgainButton.GetComponentInChildren<TextMeshProUGUI>().text = "Continue";
                }
            }
            else
            {
                resultText.text = "You failed the quiz. Game Over.";
                if (tryAgainButton != null)
                {
                    tryAgainButton.GetComponentInChildren<TextMeshProUGUI>().text = "Try Again";
                }
            }
        }
    }

    private void RespawnPlayer()
    {
        if (quizGamePanel != null)
            quizGamePanel.SetActive(false);
        
        if (quizPassed)
        {
            // Player passed, allow them to continue
            isHandlingDeath = false;
            isQuizActive = false;
            // Reset the player's state
            var player = FindFirstObjectByType<PlayerCharacterController>();
            if (player != null)
            {
                var health = player.GetComponent<Health>();
                if (health != null)
                {
                    health.Heal(health.MaxHealth);
                }
            }
        }
        else
        {
            // Player failed, show game over screen or restart level
            isQuizActive = false;
            EventManager.Broadcast(EventsGame.GameOverEvent);
        }
    }

    // Call this method when the player wants to try again or continue
    public void HideQuizAndRespawn()
    {
        RespawnPlayer();
    }
} 