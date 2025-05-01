using UnityEngine;
using TMPro;

public class QuestionDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Transform answersContainer;
    [SerializeField] private GameObject answerPrefab;

    public void SetQuestion(Question question)
    {
        questionText.text = question.Info;

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
            if (answerText != null)
            {
                answerText.text = answer.Info;
                if (answer.IsCorrect)
                {
                    answerText.color = Color.green;
                }
            }
        }
    }
} 