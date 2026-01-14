using UnityEngine;

using UnityEngine.AI;

public class enemy : MonoBehaviour
{
    public Animator animator;
    public NavMeshAgent agent;
    public float speed = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!agent.enabled)
        return;

        Vector3 targetPosition = Camera.main.transform.position;
        agent.SetDestination(targetPosition);
        agent.speed = speed;
    }

    public void Kill()
    {
        agent.enabled = false;
        animator.SetTrigger("death");
    } 

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
