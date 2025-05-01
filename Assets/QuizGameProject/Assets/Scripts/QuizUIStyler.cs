using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]
public class QuizUIStyler : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    [SerializeField] private Color buttonColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color buttonHoverColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color textColor = Color.white;

    [Header("Font Sizes")]
    [SerializeField] private int questionFontSize = 24;
    [SerializeField] private int answerFontSize = 18;
    [SerializeField] private int resultFontSize = 20;

    [Header("Spacing")]
    [SerializeField] private float questionMargin = 20f;
    [SerializeField] private float answerSpacing = 10f;
    [SerializeField] private float buttonPadding = 10f;

    private void OnEnable()
    {
        ApplyStyles();
    }

    private void OnValidate()
    {
        ApplyStyles();
    }

    private void ApplyStyles()
    {
        // Get references to all UI elements
        Image panel = GetComponent<Image>();
        TextMeshProUGUI questionText = transform.Find("QuestionText")?.GetComponent<TextMeshProUGUI>();
        Transform answersContainer = transform.Find("AnswersContainer");
        TextMeshProUGUI resultText = transform.Find("ResultText")?.GetComponent<TextMeshProUGUI>();
        Button tryAgainButton = transform.Find("TryAgainButton")?.GetComponent<Button>();

        // Apply panel styles
        if (panel != null)
        {
            panel.color = panelColor;
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.2f, 0.2f);
            panelRect.anchorMax = new Vector2(0.8f, 0.8f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }

        // Apply question text styles
        if (questionText != null)
        {
            questionText.color = textColor;
            questionText.fontSize = questionFontSize;
            questionText.alignment = TextAlignmentOptions.Center;
            RectTransform questionRect = questionText.GetComponent<RectTransform>();
            questionRect.anchorMin = new Vector2(0f, 0.7f);
            questionRect.anchorMax = new Vector2(1f, 1f);
            questionRect.offsetMin = new Vector2(questionMargin, questionMargin);
            questionRect.offsetMax = new Vector2(-questionMargin, -questionMargin);
        }

        // Apply answers container styles
        if (answersContainer != null)
        {
            RectTransform containerRect = answersContainer.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0f, 0.3f);
            containerRect.anchorMax = new Vector2(1f, 0.7f);
            containerRect.offsetMin = new Vector2(questionMargin, questionMargin);
            containerRect.offsetMax = new Vector2(-questionMargin, -questionMargin);

            // Apply styles to all answer buttons
            foreach (Transform child in answersContainer)
            {
                Button button = child.GetComponent<Button>();
                TextMeshProUGUI text = child.GetComponentInChildren<TextMeshProUGUI>();

                if (button != null)
                {
                    ColorBlock colors = button.colors;
                    colors.normalColor = buttonColor;
                    colors.highlightedColor = buttonHoverColor;
                    button.colors = colors;

                    // Apply spacing to the button
                    RectTransform buttonRect = button.GetComponent<RectTransform>();
                    buttonRect.offsetMin = new Vector2(0, answerSpacing);
                    buttonRect.offsetMax = new Vector2(0, -answerSpacing);
                }

                if (text != null)
                {
                    text.color = textColor;
                    text.fontSize = answerFontSize;
                    text.alignment = TextAlignmentOptions.Center;
                }
            }
        }

        // Apply result text styles
        if (resultText != null)
        {
            resultText.color = textColor;
            resultText.fontSize = resultFontSize;
            resultText.alignment = TextAlignmentOptions.Center;
            RectTransform resultRect = resultText.GetComponent<RectTransform>();
            resultRect.anchorMin = new Vector2(0f, 0.1f);
            resultRect.anchorMax = new Vector2(1f, 0.2f);
            resultRect.offsetMin = new Vector2(questionMargin, 0);
            resultRect.offsetMax = new Vector2(-questionMargin, 0);
        }

        // Apply try again button styles
        if (tryAgainButton != null)
        {
            Image buttonImage = tryAgainButton.GetComponent<Image>();
            TextMeshProUGUI buttonText = tryAgainButton.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonImage != null)
            {
                buttonImage.color = buttonColor;
            }

            if (buttonText != null)
            {
                buttonText.color = textColor;
                buttonText.fontSize = answerFontSize;
                buttonText.alignment = TextAlignmentOptions.Center;
                buttonText.text = "Try Again";
            }

            RectTransform buttonRect = tryAgainButton.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.4f, 0f);
            buttonRect.anchorMax = new Vector2(0.6f, 0.1f);
            buttonRect.offsetMin = new Vector2(0, buttonPadding);
            buttonRect.offsetMax = new Vector2(0, -buttonPadding);
        }
    }
} 