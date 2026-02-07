using UnityEngine;
using UnityEngine.AI;

public class enemy : MonoBehaviour
{
    public Animator animator;
    public NavMeshAgent agent;
    public float speed = 0.4f; 
    
    [Header("Audio")]
    public AudioClip[] deathSounds;

    // BREACH LOGIC VARIABLES
    private bool isBreaching = false;
    private bool isFalling = false;
    private Vector3 breachTargetPosition;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        
        // Register enemy only after breach complete (moved to CompleteBreach)
    }

    void OnDestroy()
    {
        // Only unregister if we were fully breached/active
        // Simple check: if we are not breaching anymore, we probably counted as active.
        // OR better: rely on a flag.
        if (!isBreaching && !isFalling)
        {
             FrostEffectController.ActiveEnemyCount--;
        }
        else
        {
            // If died while breaching/falling, we never incremented, so don't decrement.
        }
    }

    void Update()
    {
        // 1. PHASE-IN LOGIC (Breaching & Falling)
        if (isBreaching)
        {
            // STAP 1: Door de muur komen
            if (!isFalling)
            {
                transform.position = Vector3.MoveTowards(transform.position, breachTargetPosition, speed * Time.deltaTime);

                if (Vector3.Distance(transform.position, breachTargetPosition) < 0.1f)
                {
                    StartFalling();
                }
            }
            // STAP 2: Naar beneden glijden
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, breachTargetPosition, (speed * 2f) * Time.deltaTime);

                if (Vector3.Distance(transform.position, breachTargetPosition) < 0.1f)
                {
                    CompleteBreach();
                }
            }
            return; 
        }

        // 2. NORMAL LOGIC (NavMesh)
        if (!agent.enabled || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            return;

        if (Camera.main != null)
        {
            agent.SetDestination(Camera.main.transform.position);
            agent.speed = speed;
        }
    }

    public void InitializeBreach(Vector3 targetRoomPos)
    {
        isBreaching = true;
        isFalling = false;
        agent.enabled = false; 
        
        // Disable physics/collision during breach
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        breachTargetPosition = targetRoomPos;
        // Keep height same as spawn to ensure horizontal entry
        breachTargetPosition.y = transform.position.y;
    }

    private void StartFalling()
    {
        // Find floor directly below
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10.0f, NavMesh.AllAreas))
        {
            isFalling = true;
            breachTargetPosition = hit.position; 
        }
        else
        {
            CompleteBreach(); // Failsafe
        }
    }

    private void CompleteBreach()
    {
        isBreaching = false;
        isFalling = false;
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
        
        agent.enabled = true; 
        
        // Snap to floor
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        // Count as "Active" only now (inside the room)
        FrostEffectController.ActiveEnemyCount++;
    }

    public void Kill()
    {
        agent.enabled = false;
        isBreaching = false; 
        isFalling = false;
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false; 
        
        // Play Random Death Sound
        if (deathSounds != null && deathSounds.Length > 0)
        {
            AudioClip clip = deathSounds[Random.Range(0, deathSounds.Length)];
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }
        
        // Note: Do not decrement counter here, wait for OnDestroy
        // This ensures the frost stays until the body disappears (if you destroy it later)

        // ADD SCORE
        GameManager gm = FindFirstObjectByType<GameManager>();
        if(gm != null) gm.AddScore(10); // 10 points per kill

        if(animator) animator.SetTrigger("death");
        else Destroy(); 
    } 

    public void Destroy()
    {
        Destroy(gameObject);
    }
}