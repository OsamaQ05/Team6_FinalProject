using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizUIPrefab : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Transform answersContainer;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private GameObject answerButtonPrefab;

    [Header("Components")]
    [SerializeField] private QuestionGeneratorUI questionGenerator;
    [SerializeField] private DeathQuizManager quizManager;

    private void Awake()
    {
        // Set up the DeathQuizManager references
        if (quizManager != null)
        {
            quizManager.quizGamePanel = quizPanel;
            quizManager.questionGeneratorUI = questionGenerator;
            quizManager.resultText = resultText;
            quizManager.tryAgainButton = tryAgainButton;
        }

        // Set up the QuestionGeneratorUI references
        if (questionGenerator != null)
        {
            questionGenerator.questionsContainer = answersContainer;
            questionGenerator.questionPrefab = answerButtonPrefab;
        }

        // Make sure the quiz panel is hidden at start
        if (quizPanel != null)
            quizPanel.SetActive(false);
    }
} 