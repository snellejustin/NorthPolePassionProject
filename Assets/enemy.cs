using UnityEngine;
using UnityEngine.AI;

public class enemy : MonoBehaviour
{
    public Animator animator;
    public NavMeshAgent agent;
    public float speed = 0.4f; 
    
    [Header("Audio")]
    public AudioClip[] deathSounds;

    private bool isBreaching = false;
    private bool isFalling = false;
    private Vector3 breachTargetPosition;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        
    }

    void OnDestroy()
    {
        if (!isBreaching && !isFalling)
        {
             FrostEffectController.ActiveEnemyCount--;
        }
        else
        {
        }
    }

    void Update()
    {
        if (isBreaching)
        {
            if (!isFalling)
            {
                transform.position = Vector3.MoveTowards(transform.position, breachTargetPosition, speed * Time.deltaTime);

                if (Vector3.Distance(transform.position, breachTargetPosition) < 0.1f)
                {
                    StartFalling();
                }
            }
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
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        breachTargetPosition = targetRoomPos;
        breachTargetPosition.y = transform.position.y;
    }

    private void StartFalling()
    {
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10.0f, NavMesh.AllAreas))
        {
            isFalling = true;
            breachTargetPosition = hit.position; 
        }
        else
        {
            CompleteBreach(); 
        }
    }

    private void CompleteBreach()
    {
        isBreaching = false;
        isFalling = false;
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
        
        agent.enabled = true; 
        
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        FrostEffectController.ActiveEnemyCount++;
    }

    private bool isDead = false;

    public void Kill()
    {
        if (isDead) return;
        isDead = true;

        agent.enabled = false;
        isBreaching = false; 
        isFalling = false;
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false; 
        
        if (deathSounds != null && deathSounds.Length > 0)
        {
            AudioClip clip = deathSounds[Random.Range(0, deathSounds.Length)];
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }
        
        GameManager gm = FindFirstObjectByType<GameManager>();
        if(gm != null) gm.AddScore(10); 

        if(animator) animator.SetTrigger("death");
        else Destroy(); 
    } 

    public void Destroy()
    {
        Destroy(gameObject);
    }
}