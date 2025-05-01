using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace QuizSystem.Editor
{
    public class QuizSystemInstaller : EditorWindow
    {
        [MenuItem("Tools/Install Quiz System")]
        public static void InstallQuizSystem()
        {
            // Create Prefabs directory if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/QuizGameProject/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/QuizGameProject", "Prefabs");
            }

            // Create the quiz UI prefab
            GameObject quizUIPrefab = new GameObject("QuizUI");
            Canvas canvas = quizUIPrefab.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            quizUIPrefab.AddComponent<CanvasScaler>();
            quizUIPrefab.AddComponent<GraphicRaycaster>();

            // Create the main panel
            GameObject panel = new GameObject("QuizPanel");
            panel.transform.SetParent(quizUIPrefab.transform, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.2f, 0.2f);
            panelRect.anchorMax = new Vector2(0.8f, 0.8f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            panel.AddComponent<QuizUIPrefab>();

            // Create question text
            GameObject questionText = new GameObject("QuestionText");
            questionText.transform.SetParent(panel.transform, false);
            RectTransform questionRect = questionText.AddComponent<RectTransform>();
            questionRect.anchorMin = new Vector2(0f, 0.7f);
            questionRect.anchorMax = new Vector2(1f, 1f);
            questionRect.offsetMin = new Vector2(20f, 20f);
            questionRect.offsetMax = new Vector2(-20f, -20f);
            TextMeshProUGUI questionTMP = questionText.AddComponent<TextMeshProUGUI>();
            questionTMP.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            questionTMP.color = Color.white;
            questionTMP.fontSize = 24;
            questionTMP.alignment = TextAlignmentOptions.Center;

            // Create answers container
            GameObject answersContainer = new GameObject("AnswersContainer");
            answersContainer.transform.SetParent(panel.transform, false);
            RectTransform containerRect = answersContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0f, 0.3f);
            containerRect.anchorMax = new Vector2(1f, 0.7f);
            containerRect.offsetMin = new Vector2(20f, 20f);
            containerRect.offsetMax = new Vector2(-20f, -20f);

            // Create result text
            GameObject resultText = new GameObject("ResultText");
            resultText.transform.SetParent(panel.transform, false);
            RectTransform resultRect = resultText.AddComponent<RectTransform>();
            resultRect.anchorMin = new Vector2(0f, 0.1f);
            resultRect.anchorMax = new Vector2(1f, 0.2f);
            resultRect.offsetMin = new Vector2(20f, 0);
            resultRect.offsetMax = new Vector2(-20f, 0);
            TextMeshProUGUI resultTMP = resultText.AddComponent<TextMeshProUGUI>();
            resultTMP.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            resultTMP.color = Color.white;
            resultTMP.fontSize = 20;
            resultTMP.alignment = TextAlignmentOptions.Center;

            // Create try again button
            GameObject tryAgainButton = new GameObject("TryAgainButton");
            tryAgainButton.transform.SetParent(panel.transform, false);
            RectTransform buttonRect = tryAgainButton.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.4f, 0f);
            buttonRect.anchorMax = new Vector2(0.6f, 0.1f);
            buttonRect.offsetMin = new Vector2(0, 10f);
            buttonRect.offsetMax = new Vector2(0, -10f);
            Image buttonImage = tryAgainButton.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            Button button = tryAgainButton.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            button.colors = colors;

            // Create button text
            GameObject buttonText = new GameObject("Text");
            buttonText.transform.SetParent(tryAgainButton.transform, false);
            RectTransform textRect = buttonText.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            TextMeshProUGUI textTMP = buttonText.AddComponent<TextMeshProUGUI>();
            textTMP.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            textTMP.color = Color.white;
            textTMP.fontSize = 18;
            textTMP.alignment = TextAlignmentOptions.Center;
            textTMP.text = "Try Again";

            // Create answer button prefab
            GameObject answerButtonPrefab = new GameObject("AnswerButton");
            RectTransform answerButtonRect = answerButtonPrefab.AddComponent<RectTransform>();
            answerButtonRect.sizeDelta = new Vector2(0, 40f);
            Image answerButtonImage = answerButtonPrefab.AddComponent<Image>();
            answerButtonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            Button answerButton = answerButtonPrefab.AddComponent<Button>();
            ColorBlock answerColors = answerButton.colors;
            answerColors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            answerColors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            answerButton.colors = answerColors;

            // Create answer button text
            GameObject answerButtonText = new GameObject("Text");
            answerButtonText.transform.SetParent(answerButtonPrefab.transform, false);
            RectTransform answerTextRect = answerButtonText.AddComponent<RectTransform>();
            answerTextRect.anchorMin = Vector2.zero;
            answerTextRect.anchorMax = Vector2.one;
            answerTextRect.offsetMin = Vector2.zero;
            answerTextRect.offsetMax = Vector2.zero;
            TextMeshProUGUI answerTextTMP = answerButtonText.AddComponent<TextMeshProUGUI>();
            answerTextTMP.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            answerTextTMP.color = Color.white;
            answerTextTMP.fontSize = 18;
            answerTextTMP.alignment = TextAlignmentOptions.Center;

            // Add required components
            quizUIPrefab.AddComponent<QuestionGeneratorUI>();
            quizUIPrefab.AddComponent<DeathQuizManager>();

            // Save the answer button prefab
            PrefabUtility.SaveAsPrefabAsset(answerButtonPrefab, "Assets/QuizGameProject/Prefabs/AnswerButton.prefab");
            Object.DestroyImmediate(answerButtonPrefab);

            // Save the quiz UI prefab
            PrefabUtility.SaveAsPrefabAsset(quizUIPrefab, "Assets/QuizGameProject/Prefabs/QuizUI.prefab");

            // Add to current scene
            GameObject quizInstance = PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/QuizGameProject/Prefabs/QuizUI.prefab")
            ) as GameObject;
            quizInstance.name = "QuizSystem";

            // Mark the scene as dirty to ensure changes are saved
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            // Clean up the temporary prefab object
            EditorApplication.delayCall += () => {
                if (quizUIPrefab != null)
                {
                    Object.DestroyImmediate(quizUIPrefab);
                }
            };

            Debug.Log("Quiz system installed successfully!");
        }
    }
} 