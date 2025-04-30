using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.FPS.Game;

public class StatueInteraction : MonoBehaviour
{
    public int statueNumber;
    public Material cleansedMaterial;
    public ParticleSystem cleanseEffect;
    public List<ParticleSystem> fireParticles = new List<ParticleSystem>(); 
    public AudioSource cleanseSound;
    public string virtueName; 
    public float interactionDistance = 5f;

    // Enum for manager selection (will show as radio buttons in Inspector)
    public enum ManagerType
    {
        Medieval,
        Samurai
    }
    
    [Header("Manager Selection")]
    [Tooltip("Select which manager type to use")]
    public ManagerType selectedManager = ManagerType.Medieval;

    [Header("Interact Prompt Settings")]
    public TextMeshProUGUI interactPromptText; // ONLY the TMP text (no parent UI!)
    
    [Tooltip("Custom prompt text (leave empty for default)")]
    public string customPromptText = "";

    private bool isCleansed = false;
    private static int virtuesRestored = 0;
    private Renderer statueRenderer;
    
    // Manager references
    private MedievalManager medievalManager;
    private SamuraiGameManager samuraiManager;

    private void Start()
    {
        // Find the appropriate manager based on selection
        switch (selectedManager)
        {
            case ManagerType.Medieval:
                medievalManager = FindObjectOfType<MedievalManager>();
                if (medievalManager == null)
                    Debug.LogError("MedievalManager not found but StatueInteraction is set to use it!");
                break;
                
            case ManagerType.Samurai:
                samuraiManager = FindObjectOfType<SamuraiGameManager>();
                if (samuraiManager == null)
                    Debug.LogError("SamuraiGameManager not found but StatueInteraction is set to use it!");
                break;
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

        // Notify the selected manager
        switch (selectedManager)
        {
            case ManagerType.Medieval:
                if (medievalManager != null)
                    medievalManager.OnStatueCleansed(statueNumber);
                break;
                
            case ManagerType.Samurai:
                if (samuraiManager != null)
                    samuraiManager.OnStatueCleansed(statueNumber);
                break;
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
            string promptAction;
            
            if (!string.IsNullOrEmpty(customPromptText))
            {
                promptAction = customPromptText;
            }
            else
            {
                // Default text based on manager type
                promptAction = selectedManager == ManagerType.Medieval ? 
                    "Light Torch" : "Cleanse Statue";
            }
            
            interactPromptText.text = $"<b>Press [ E ] to {promptAction}</b>"; 
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