using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.FPS.Game;

public class StatueInteraction : MonoBehaviour
{
    public int statueNumber;
    private SamuraiGameManager gameManager;
    public Material cleansedMaterial;
    public ParticleSystem cleanseEffect;
    public List<ParticleSystem> fireParticles = new List<ParticleSystem>(); 
    public AudioSource cleanseSound;
    public string virtueName; 
    public float interactionDistance = 5f;

    [Header("Interact Prompt Settings")]
    public TextMeshProUGUI interactPromptText; // ONLY the TMP text (no parent UI!)

    private bool isCleansed = false;
    private static int virtuesRestored = 0;
    private Renderer statueRenderer;

    private void Start()
    {
        gameManager = FindObjectOfType<SamuraiGameManager>();
        statueRenderer = GetComponent<Renderer>();
        gameObject.tag = "Statue";

        if (interactPromptText != null)
            interactPromptText.alpha = 0f; // 🔥 Hide at start (alpha = 0)

        if (fireParticles != null && fireParticles.Count > 0)
        {
            foreach (ParticleSystem fire in fireParticles)
            {
                if (fire != null)
                    fire.Stop();
            }
        }
    }

   private void Update()
{
    if (isCleansed)
        return; // just stop Update logic, don't hide prompt here

    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, interactionDistance))
    {
        if (hit.collider != null && hit.collider.gameObject == this.gameObject)
        {
            ShowInteractPrompt();

            if (Input.GetKeyDown(KeyCode.E))
            {
                CleanseStatue();
            }
        }
        else
        {
            HideInteractPrompt();
        }
    }
    else
    {
        HideInteractPrompt();
    }
}


   void CleanseStatue()
{
    isCleansed = true;
    virtuesRestored++;

    if (gameManager != null)
        gameManager.OnStatueCleansed(statueNumber);

    if (cleansedMaterial != null && statueRenderer != null)
        statueRenderer.material = cleansedMaterial;

    if (cleanseEffect != null)
        cleanseEffect.Play();

    if (cleanseSound != null)
        cleanseSound.Play();

    if (fireParticles != null && fireParticles.Count > 0)
    {
        foreach (ParticleSystem fire in fireParticles)
        {
            if (fire != null)
                fire.Play();
        }
    }

    DisplayVirtueRestoredMessage();
    HideInteractPrompt(); // 🛠 Hide here once it's cleansed
}


    void DisplayVirtueRestoredMessage()
    {
        DisplayMessageEvent displayMessage = Events.DisplayMessageEvent;
        displayMessage.Message = $"{virtueName} Restored!";
        displayMessage.DelayBeforeDisplay = 0f;
        EventManager.Broadcast(displayMessage);
    }

    void ShowInteractPrompt()
    {
        if (interactPromptText != null)
        {
            interactPromptText.alpha = 1f; // Set visible 🔥
            interactPromptText.text = "<b>Press [ E ] to Cleanse</b>";
        }
    }

    void HideInteractPrompt()
    {
        if (interactPromptText != null)
        {
            interactPromptText.alpha = 0f; // Hide 🔥
        }
    }
}
