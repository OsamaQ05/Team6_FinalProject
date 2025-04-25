using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
//
public class KAI_MessageTrigger : MonoBehaviour
{
    [TextArea(3, 10)]
    public string dialogueText;

    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueTMP;
    public AudioSource dialogueAudio;
    public float displayDuration = 5f;
    public float typingSpeed = 0.03f;

    private bool hasSpoken = false;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        if (dialoguePanel != null)
        {
            canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = dialoguePanel.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
            dialoguePanel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Play") && !hasSpoken)
        {
            hasSpoken = true;
            StartCoroutine(ShowMessage());
        }
    }

    private IEnumerator ShowMessage()
    {
        dialoguePanel.SetActive(true);
        canvasGroup.alpha = 1f;
        dialogueTMP.text = "";

        if (dialogueAudio != null)
            dialogueAudio.Play();

        foreach (char c in dialogueText)
        {
            dialogueTMP.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(displayDuration);

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float fadeDuration = 1f;
        float startAlpha = canvasGroup.alpha;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        dialoguePanel.SetActive(false);
    }
}
