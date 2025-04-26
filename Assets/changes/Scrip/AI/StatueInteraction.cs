using UnityEngine;
using TMPro;

public class StatueInteraction : MonoBehaviour
{
    public Material cleansedMaterial;
    public ParticleSystem cleanseEffect;
    public AudioSource cleanseSound;
    public string virtueName; // Example: "Honor", "Courage", "Loyalty"
    public TextMeshProUGUI messageText;
    public float messageDuration = 5f;
    public float interactionDistance = 5f;

    private bool isCleansed = false;
    private static int virtuesRestored = 0;
    private Renderer statueRenderer;

    private void Start()
    {
        statueRenderer = GetComponent<Renderer>();
        gameObject.tag = "Statue"; // Ensure the statue has the correct tag
    }

    private void Update()
    {
        if (isCleansed) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider != null && hit.collider.gameObject == this.gameObject)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    CleanseStatue();
                }
            }
        }
    }

    void CleanseStatue()
    {
        if (cleansedMaterial != null && statueRenderer != null)
        {
            statueRenderer.material = cleansedMaterial;
        }

        if (cleanseEffect != null)
        {
            cleanseEffect.Play();
        }

        if (cleanseSound != null)
        {
            cleanseSound.Play();
        }

        if (messageText != null)
        {
            messageText.text = $"{virtueName} Restored!";
            Invoke(nameof(ClearMessage), messageDuration);
        }

        isCleansed = true;
        virtuesRestored++;

        if (virtuesRestored >= 3)
        {
            // All virtues restored! You can trigger end level stuff here
            Debug.Log("All virtues restored! Level Complete!");
        }
    }

    void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }
    }
}
