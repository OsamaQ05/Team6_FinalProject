using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class QuizSystemInstaller : EditorWindow
{
    [MenuItem("Tools/Install Quiz System")]
    public static void InstallQuizSystem()
    {
        // Create the quiz UI prefab
        GameObject quizUIPrefab = new GameObject("QuizUI");
        quizUIPrefab.AddComponent<Canvas>();
        quizUIPrefab.AddComponent<CanvasScaler>();
        quizUIPrefab.AddComponent<GraphicRaycaster>();

        // Create the main panel
        GameObject panel = new GameObject("QuizPanel");
        panel.transform.SetParent(quizUIPrefab.transform, false);
        panel.AddComponent<RectTransform>();
        panel.AddComponent<Image>();
        panel.AddComponent<QuizUIPrefab>();

        // Create question text
        GameObject questionText = new GameObject("QuestionText");
        questionText.transform.SetParent(panel.transform, false);
        questionText.AddComponent<RectTransform>();
        questionText.AddComponent<TextMeshProUGUI>();

        // Create answers container
        GameObject answersContainer = new GameObject("AnswersContainer");
        answersContainer.transform.SetParent(panel.transform, false);
        answersContainer.AddComponent<RectTransform>();

        // Create result text
        GameObject resultText = new GameObject("ResultText");
        resultText.transform.SetParent(panel.transform, false);
        resultText.AddComponent<RectTransform>();
        resultText.AddComponent<TextMeshProUGUI>();

        // Create try again button
        GameObject tryAgainButton = new GameObject("TryAgainButton");
        tryAgainButton.transform.SetParent(panel.transform, false);
        tryAgainButton.AddComponent<RectTransform>();
        tryAgainButton.AddComponent<Image>();
        tryAgainButton.AddComponent<Button>();

        // Create answer button prefab
        GameObject answerButtonPrefab = new GameObject("AnswerButton");
        answerButtonPrefab.AddComponent<RectTransform>();
        answerButtonPrefab.AddComponent<Image>();
        answerButtonPrefab.AddComponent<Button>();
        GameObject answerText = new GameObject("Text");
        answerText.transform.SetParent(answerButtonPrefab.transform, false);
        answerText.AddComponent<RectTransform>();
        answerText.AddComponent<TextMeshProUGUI>();

        // Add required components
        quizUIPrefab.AddComponent<QuestionGeneratorUI>();
        quizUIPrefab.AddComponent<DeathQuizManager>();

        // Save the prefab
        PrefabUtility.SaveAsPrefabAsset(quizUIPrefab, "Assets/QuizGameProject/Prefabs/QuizUI.prefab");

        // Add to current scene
        GameObject quizInstance = Instantiate(quizUIPrefab);
        quizInstance.name = "QuizSystem";

        // Save the scene
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        Debug.Log("Quiz system installed successfully!");
    }
} 