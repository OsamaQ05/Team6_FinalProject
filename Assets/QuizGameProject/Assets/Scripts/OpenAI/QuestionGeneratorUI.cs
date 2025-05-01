using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class QuestionGeneratorUI : MonoBehaviour
{
    [SerializeField] private OpenAIQuestionGenerator questionGenerator;
    [SerializeField] private Button generateButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Transform questionsContainer;
    [SerializeField] private GameObject questionPrefab;

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

    private void Start()
    {
        generateButton.onClick.AddListener(GenerateQuestions);
    }

    private async void GenerateQuestions()
    {
        generateButton.interactable = false;
        statusText.text = "Generating questions...";

        try
        {
            var questions = await questionGenerator.GenerateQuestions(topics, historicalEras);
            
            // Clear existing questions
            foreach (Transform child in questionsContainer)
            {
                Destroy(child.gameObject);
            }

            // Display new questions
            foreach (var question in questions)
            {
                GameObject questionObj = Instantiate(questionPrefab, questionsContainer);
                QuestionDisplay display = questionObj.GetComponent<QuestionDisplay>();
                if (display != null)
                {
                    display.SetQuestion(question);
                }
            }

            statusText.text = $"Generated {questions.Count} questions successfully!";
        }
        catch (System.Exception e)
        {
            statusText.text = $"Error: {e.Message}";
        }
        finally
        {
            generateButton.interactable = true;
        }
    }
} 