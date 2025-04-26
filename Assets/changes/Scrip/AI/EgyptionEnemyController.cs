using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Unity.FPS.Game;



public class EnemyFollow : MonoBehaviour
{
    Animator anim;

    [Header("Player Tracking")]
    public Transform player;
    private NavMeshAgent agent;

    [Header("Detection Settings")]
    public float detectionRange = 20f;
    public float visionRange = 40f;
    public float fieldOfView = 90f; // degrees
    public float crouchDetectionMultiplier = 0.5f; // 50% harder to detect

    [Header("Look Speed")]
    public float lookSpeed = 5f;

    [Header("Game Over")]
    public string detectionMessage = "You've been spotted!";
    public float delayBeforeMessage = 1.5f;
    public float sceneRestartDelay = 3f;

    [Header("Patrol Settings")]
    [SerializeField] public Transform[] patrolPoints;
    public float pathReachingRadius = 1.5f;
    public bool inversePatrol = false;

    [Header("Alert UI & Audio")]
    public GameObject alertIcon;
    public AudioClip detectionSound;
    private AudioSource audioSource;

    private bool playerDetected = false;
    private int currentPatrolIndex = 0;
    private bool goingForward = true;
    public bool isCroched ;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        alertIcon?.SetActive(false);
    }

    void Update()
    {
        if (playerDetected)
        {
            FacePlayer();
            anim.SetBool("isDetected", true);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        bool inView = angle < fieldOfView / 2f;

        float baseRange = inView ? visionRange : detectionRange;

        // Apply crouch stealth modifier
        float finalDetectionRange = isCroched
            ? baseRange * crouchDetectionMultiplier
            : baseRange;

        if (distanceToPlayer <= finalDetectionRange)
        {
            RaycastHit hit;
            Vector3 rayOrigin = transform.position + Vector3.up;

            if (Physics.Raycast(rayOrigin, directionToPlayer, out hit, finalDetectionRange))
            {
                if (hit.transform == player)
                {
                    OnPlayerDetected();
                    return;
                }
            }
        }

        // Patrol Logic
        if (patrolPoints.Length > 0)
        {
            Vector3 patrolTarget = patrolPoints[currentPatrolIndex].position;
            agent.SetDestination(patrolTarget);

            float distanceToPoint = Vector3.Distance(transform.position, patrolTarget);
            if (distanceToPoint <= pathReachingRadius)
            {
                UpdatePatrolIndex();
            }
        }
    }

    void UpdatePatrolIndex()
    {
        if (inversePatrol)
        {
            if (goingForward)
            {
                currentPatrolIndex++;
                if (currentPatrolIndex >= patrolPoints.Length)
                {
                    currentPatrolIndex = patrolPoints.Length - 2;
                    goingForward = false;
                }
            }
            else
            {
                currentPatrolIndex--;
                if (currentPatrolIndex < 0)
                {
                    currentPatrolIndex = 1;
                    goingForward = true;
                }
            }
        }
        else
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookSpeed);
    }

    void OnPlayerDetected()
    {
        alertIcon?.SetActive(true);
        if (playerDetected) return;

        playerDetected = true;
        agent.isStopped = true;

        Debug.Log("Player detected! Stopping and looking at player.");

        // Play detection sound
        if (audioSource != null && detectionSound != null)
        {
            audioSource.PlayOneShot(detectionSound);
        }

        // Send detection message
        DisplayMessageEvent messageEvent = Events.DisplayMessageEvent;
        messageEvent.Message = detectionMessage;
        messageEvent.DelayBeforeDisplay = delayBeforeMessage;
        EventManager.Broadcast(messageEvent);

        // Restart scene after delay
        Invoke(nameof(RestartScene), sceneRestartDelay);
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.yellow;
        Vector3 forward = transform.forward * visionRange;
        Vector3 leftRay = Quaternion.Euler(0, -fieldOfView / 2f, 0) * forward;
        Vector3 rightRay = Quaternion.Euler(0, fieldOfView / 2f, 0) * forward;

        Gizmos.DrawRay(transform.position, leftRay);
        Gizmos.DrawRay(transform.position, rightRay);
        Gizmos.DrawWireSphere(transform.position, detectionRange); // base detection
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f); // transparent orange
        Gizmos.DrawWireSphere(transform.position, visionRange); // extended vision
    }

}

