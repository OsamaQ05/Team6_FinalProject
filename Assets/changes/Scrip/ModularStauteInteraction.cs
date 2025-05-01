using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.FPS.Game;

public class ModularStatueInteraction : MonoBehaviour
{
    public int statueNumber;
    public Material cleansedMaterial;
    public ParticleSystem cleanseEffect;
    public List<ParticleSystem> fireParticles = new List<ParticleSystem>(); 
    public AudioSource cleanseSound;
    public string virtueName; 
    public float interactionDistance = 5f;
    
    [Tooltip("Check this if this statue is in a Medieval scene")]
    public bool UseMedievalManager = false;
    
    [Tooltip("Check this if this statue is in a Samurai scene")]
    public bool UseSamuraiManager = false;
    
    [Tooltip("Custom interact prompt text (leave empty for default)")]
    public string CustomInteractPrompt = "";

    [Header("Interact Prompt Settings")]
    public TextMeshProUGUI interactPromptText; // ONLY the TMP text (no parent UI!)

    private bool isCleansed = false;
    private static int virtuesRestored = 0;
    private Renderer statueRenderer;
    
    // References to managers
    private MedievalManager medievalManager;
    private SamuraiGameManager samuraiManager;

    private void Start()
    {
        // Try to find appropriate manager based on settings
        if (UseMedievalManager)
        {
            medievalManager = FindFirstObjectByType<MedievalManager>();
            if (medievalManager == null)
                Debug.LogWarning("ModularStatueInteraction set to use MedievalManager but none was found!");
        }
        else if (UseSamuraiManager)
        {
            samuraiManager = FindFirstObjectByType<SamuraiGameManager>();
            if (samuraiManager == null)
                Debug.LogWarning("ModularStatueInteraction set to use SamuraiGameManager but none was found!");
        }
        else
        {
            // Auto-detect manager if not specified
            medievalManager = FindFirstObjectByType<MedievalManager>();
            if (medievalManager != null)
            {
                UseMedievalManager = true;
            }
            else
            {
                samuraiManager = FindFirstObjectByType<SamuraiGameManager>();
                if (samuraiManager != null)
                {
                    UseSamuraiManager = true;
                }
                else
                {
                    Debug.LogError("ModularStatueInteraction could not find either MedievalManager or SamuraiGameManager!");
                }
            }
        }
        
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

        // Notify the appropriate manager
        if (UseMedievalManager && medievalManager != null)
        {
            medievalManager.OnStatueCleansed(statueNumber);
            Debug.Log("Notified MedievalManager of statue cleansed");
        }
        else if (UseSamuraiManager && samuraiManager != null)
        {
            samuraiManager.OnStatueCleansed(statueNumber);
            Debug.Log("Notified SamuraiGameManager of statue cleansed");
        }
        else
        {
            Debug.LogError("No valid game manager found to notify about statue cleansing!");
        }

        // Visual effects
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
        // Create a new DisplayMessageEvent directly
        DisplayMessageEvent displayMessage = new DisplayMessageEvent();
        displayMessage.Message = $"{virtueName} Restored!";
        displayMessage.DelayBeforeDisplay = 0f;
        EventManager.Broadcast(displayMessage);
    }

    void ShowInteractPrompt()
    {
        if (interactPromptText != null)
        {
            interactPromptText.alpha = 1f; // Set visible 🔥
            
            // Use custom prompt if provided, otherwise use default based on manager type
            if (!string.IsNullOrEmpty(CustomInteractPrompt))
            {
                interactPromptText.text = $"<b>Press [ E ] to {CustomInteractPrompt}</b>";
            }
            else if (UseMedievalManager)
            {
                interactPromptText.text = "<b>Press [ E ] to Light Torch</b>";
            }
            else if (UseSamuraiManager)
            {
                interactPromptText.text = "<b>Press [ E ] to Cleanse Statue</b>";
            }
            else
            {
                interactPromptText.text = "<b>Press [ E ] to Interact</b>";
            }
        }
    }

    void HideInteractPrompt()
    {
        if (interactPromptText != null)
        {
            interactPromptText.alpha = 0f; // Hide ��
        }
    }
}