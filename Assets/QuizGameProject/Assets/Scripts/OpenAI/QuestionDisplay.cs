using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestionDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Transform answersContainer;
    [SerializeField] private GameObject answerPrefab;
    [SerializeField] private DeathQuizManager quizManager;

    private bool answerSubmitted = false;

    public void SetQuestion(Question question)
    {
        questionText.text = question.Info;
        answerSubmitted = false;

        // Clear existing answers
        foreach (Transform child in answersContainer)
        {
            Destroy(child.gameObject);
        }

        // Create new answer displays
        foreach (var answer in question.Answers)
        {
            GameObject answerObj = Instantiate(answerPrefab, answersContainer);
            TextMeshProUGUI answerText = answerObj.GetComponentInChildren<TextMeshProUGUI>();
            Button answerButton = answerObj.GetComponent<Button>();
            
            if (answerText != null)
            {
                answerText.text = answer.Info;
                if (answer.IsCorrect)
                {
                    answerText.color = Color.green;
                }
            }

            if (answerButton != null)
            {
                bool isCorrect = answer.IsCorrect;
                answerButton.onClick.AddListener(() => OnAnswerSelected(isCorrect));
            }
        }
    }

    private void OnAnswerSelected(bool isCorrect)
    {
        if (answerSubmitted) return;
        answerSubmitted = true;

        // Notify the quiz manager about the answer
        if (quizManager != null)
        {
            quizManager.OnAnswerSubmitted(isCorrect);
        }

        // Disable all answer buttons
        foreach (Transform child in answersContainer)
        {
            Button button = child.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
        }
    }
} 