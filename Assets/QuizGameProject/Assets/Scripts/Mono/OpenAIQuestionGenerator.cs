using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;

[System.Serializable]
public class OpenAIRequest
{
    public string model = "gpt-3.5-turbo";
    public List<Message> messages = new List<Message>();
}

[System.Serializable]
public class Message
{
    public string role = "user";
    public string content;
}

[System.Serializable]
public class OpenAIResponse
{
    public List<Choice> choices = new List<Choice>();
}

[System.Serializable]
public class Choice
{
    public Message message;
}

public class OpenAIQuestionGenerator : MonoBehaviour
{
    [SerializeField] private string apiKey;
    private const string API_URL = "https://api.openai.com/v1/chat/completions";

    public async Task<List<Question>> GenerateQuestions(string[] topics, string[] historicalEras)
    {
        var questions = new List<Question>();
        
        try
        {
            foreach (var topic in topics)
            {
                foreach (var era in historicalEras)
                {
                    var prompt = $"Generate a multiple choice question about {topic} from {era}. Include 4 answer choices and mark the correct one. Format the response as: Question: [question] A) [answer1] B) [answer2] C) [answer3] D) [answer4] Correct: [letter]";
                    
                    var response = await SendOpenAIRequest(prompt);
                    if (response != null)
                    {
                        var question = ParseQuestion(response);
                        if (question != null)
                        {
                            questions.Add(question);
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error generating questions: {e.Message}");
        }

        return questions;
    }

    private async Task<string> SendOpenAIRequest(string prompt)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("OpenAI API Key is not set! Please set it in the Inspector.");
            return null;
        }

        var request = new OpenAIRequest
        {
            messages = new List<Message> { new Message { content = prompt } }
        };

        string jsonRequest = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);

        using (UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST"))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<OpenAIResponse>(webRequest.downloadHandler.text);
                return response.choices[0].message.content;
            }
            else
            {
                Debug.LogError($"Error: {webRequest.error}");
                return null;
            }
        }
    }

    private Question ParseQuestion(string content)
    {
        try
        {
            Debug.Log($"Parsing OpenAI response: {content}");
            
            var lines = content.Split('\n');
            if (lines.Length < 6)
            {
                Debug.LogError($"Invalid response format. Expected 6 lines, got {lines.Length}");
                return null;
            }

            // Parse question
            var questionText = lines[0].Trim();
            if (questionText.StartsWith("Question:"))
            {
                questionText = questionText.Substring("Question:".Length).Trim();
            }

            // Parse answers
            var answers = new QuestionAnswer[4];
            for (int i = 1; i <= 4; i++)
            {
                if (i >= lines.Length)
                {
                    Debug.LogError($"Missing answer line {i}");
                    return null;
                }

                var line = lines[i].Trim();
                if (line.Length < 3)
                {
                    Debug.LogError($"Invalid answer format at line {i}: {line}");
                    return null;
                }

                var answerText = line.Substring(3).Trim(); // Remove "A) ", "B) ", etc.
                answers[i - 1] = new QuestionAnswer
                {
                    Info = answerText,
                    IsCorrect = false
                };
            }

            // Parse correct answer
            if (lines.Length < 6)
            {
                Debug.LogError("Missing correct answer line");
                return null;
            }

            var correctLine = lines[5].Trim();
            if (!correctLine.StartsWith("Correct:"))
            {
                Debug.LogError($"Invalid correct answer format: {correctLine}");
                return null;
            }

            var correctLetter = correctLine.Substring("Correct:".Length).Trim();
            var correctAnswer = correctLetter switch
            {
                "A" => 0,
                "B" => 1,
                "C" => 2,
                "D" => 3,
                _ => -1
            };

            if (correctAnswer < 0)
            {
                Debug.LogError($"Invalid correct answer letter: {correctLetter}");
                return null;
            }

            answers[correctAnswer].IsCorrect = true;

            // Create and configure question
            var question = ScriptableObject.CreateInstance<Question>();
            
            // Set the serialized fields using reflection
            var type = question.GetType();
            var infoField = type.GetField("_info", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var answersField = type.GetField("_answers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var useTimerField = type.GetField("_useTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var timerField = type.GetField("_timer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var addScoreField = type.GetField("_addScore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var answerTypeField = type.GetField("_answerType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            infoField.SetValue(question, questionText);
            answersField.SetValue(question, answers);
            useTimerField.SetValue(question, true);
            timerField.SetValue(question, 30);
            addScoreField.SetValue(question, 10);
            answerTypeField.SetValue(question, Question.AnswerType.Single);

            Debug.Log($"Successfully parsed question: {questionText}");
            return question;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing question: {e.Message}\nStack trace: {e.StackTrace}");
            return null;
        }
    }
} 