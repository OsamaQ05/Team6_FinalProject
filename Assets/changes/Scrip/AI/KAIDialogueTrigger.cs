using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

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
    
    // Static reference to ensure only one message shows at a time
    private static Coroutine activeShowCoroutine;
    private static Coroutine activeFadeCoroutine;
    private static MonoBehaviour activeScript;

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
        // Note: Changed from "Play" to "Player" in the original - adjust as needed
        if (other.CompareTag("Play") && !hasSpoken)
        {
            hasSpoken = true;
            
            // Stop any active dialogue from any trigger
            StopActiveDialogue();
            
            // Start this dialogue
            activeScript = this;
            activeShowCoroutine = StartCoroutine(ShowMessage());
        }
    }
    
    // Helper method to stop any active dialogue
    private void StopActiveDialogue()
    {
        if (activeScript != null)
        {
            if (activeShowCoroutine != null)
            {
                activeScript.StopCoroutine(activeShowCoroutine);
                activeShowCoroutine = null;
            }
            
            if (activeFadeCoroutine != null)
            {
                activeScript.StopCoroutine(activeFadeCoroutine);
                activeFadeCoroutine = null;
            }
        }
    }

    private IEnumerator ShowMessage()
    {
        // Reset everything to start fresh
        dialoguePanel.SetActive(true);
        canvasGroup.alpha = 1f;
        dialogueTMP.text = "";

        if (dialogueAudio != null)
        {
            if (dialogueAudio.isPlaying)
            {
                dialogueAudio.Stop();
            }
            dialogueAudio.Play();
        }

        // Typewriter effect
        foreach (char c in dialogueText)
        {
            dialogueTMP.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Display duration
        yield return new WaitForSeconds(displayDuration);

        // Start fade out
        activeFadeCoroutine = StartCoroutine(FadeOut());
        activeShowCoroutine = null;
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
        activeFadeCoroutine = null;
    }
}