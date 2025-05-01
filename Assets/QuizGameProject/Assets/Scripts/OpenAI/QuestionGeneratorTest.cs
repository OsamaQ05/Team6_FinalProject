using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class QuestionGeneratorTest : MonoBehaviour
{
    [SerializeField] private OpenAIQuestionGenerator questionGenerator;
    [SerializeField] private TextMeshProUGUI debugText;

    private string[] topics = new string[]
    {
        "Bust of Nefertiti",
        "Kusanagi Tsurugi",
        "Royal Crown (Medieval Europe)"
    };

    private string[] historicalEras = new string[]
    {
        "Ancient Egypt",
        "Feudal Japan",
        "Medieval Europe"
    };

    private async void Start()
    {
        if (questionGenerator == null)
        {
            Debug.LogError("QuestionGenerator is not assigned!");
            return;
        }

        debugText.text = "Starting question generation...";
        var questions = await questionGenerator.GenerateQuestions(topics, historicalEras);
        
        if (questions == null || questions.Count == 0)
        {
            debugText.text = "No questions were generated!";
            return;
        }

        string result = $"Generated {questions.Count} questions:\n\n";
        foreach (var question in questions)
        {
            result += $"Question: {question.Info}\n";
            foreach (var answer in question.Answers)
            {
                result += $"- {answer.Info} ({(answer.IsCorrect ? "Correct" : "Incorrect")})\n";
            }
            result += "\n";
        }

        debugText.text = result;
        Debug.Log(result);
    }
} 