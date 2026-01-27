using UnityEngine;
using UnityEngine.AI;

public class enemy : MonoBehaviour
{
    public Animator animator;
    public NavMeshAgent agent;
    public float speed = 0.4f; 

    // BREACH LOGIC VARIABLES
    private bool isBreaching = false;
    private bool isFalling = false; // Nieuwe fase voor het zakken
    private Vector3 breachTargetPosition;

    void Start()
    {
        // Auto-assign agent if missing
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        // 1. PHASE-IN LOGIC (Breaching & Falling)
        if (isBreaching)
        {
            // STAP 1: Door de muur komen
            if (!isFalling)
            {
                // Beweeg horizontaal naar binnen
                transform.position = Vector3.MoveTowards(transform.position, breachTargetPosition, speed * Time.deltaTime);

                // Zijn we door het gat heen? (afstand check)
                if (Vector3.Distance(transform.position, breachTargetPosition) < 0.1f)
                {
                    // Ja, we zijn binnen! Nu kijken waar de vloer is.
                    StartFalling();
                }
            }
            // STAP 2: Naar beneden glijden
            else
            {
                // Beweeg naar het punt op de vloer (breachTargetPosition is nu de vloer)
                // We doen speed * 2 zodat hij wat sneller valt dan dat hij kruipt (zwaartekracht effect)
                transform.position = Vector3.MoveTowards(transform.position, breachTargetPosition, (speed * 2f) * Time.deltaTime);

                if (Vector3.Distance(transform.position, breachTargetPosition) < 0.1f)
                {
                    CompleteBreach(); // We zijn geland
                }
            }
            return; 
        }

        // 2. NORMAL LOGIC (NavMesh)
        if (!agent.enabled || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            return;

        if (Camera.main != null)
        {
            Vector3 targetPosition = Camera.main.transform.position;
            agent.SetDestination(targetPosition);
            agent.speed = speed;
        }
    }

    // Called by the Spawner/Manager when created behind a wall
    public void InitializeBreach(Vector3 targetRoomPos)
    {
        isBreaching = true;
        isFalling = false; // Reset falling state
        agent.enabled = false; 
        
        // Zet physics uit zodat we niet blijven haken aan de muur
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        breachTargetPosition = targetRoomPos;
        // FORCE HORIZONTAL: Override Y to match current height so we don't climb/descend during breach
        breachTargetPosition.y = transform.position.y;
    }

    private void StartFalling()
    {
        // Zoek de vloer recht onder de enemy (tot 10 meter diep)
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10.0f, NavMesh.AllAreas))
        {
            isFalling = true;
            breachTargetPosition = hit.position; // Nieuw doel is de vloer
        }
        else
        {
            // Geen vloer gevonden? Dan maar direct aanzetten (failsafe)
            CompleteBreach();
        }
    }

    private void CompleteBreach()
    {
        isBreaching = false;
        isFalling = false;
        
        // Zet physics weer aan zodat we geraakt kunnen worden
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
        
        // Zet het brein aan
        agent.enabled = true; 
        
        // Zeker weten dat hij op de navmesh staat
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }

    public void Kill()
    {
        agent.enabled = false;
        isBreaching = false; 
        isFalling = false;
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false; 

        if(animator) animator.SetTrigger("death");
        else Destroy(); 
    } 

    public void Destroy()
    {
        Destroy(gameObject);
    }
}