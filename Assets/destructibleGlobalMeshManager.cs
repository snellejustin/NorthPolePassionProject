using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

public class destructibleGlobalMeshManager : MonoBehaviour
{
    public DestructibleGlobalMeshSpawner meshSpawner;
    public float destructionInterval = 2.0f; // Time in seconds between destruction
    
    private List<GameObject> segments = new List<GameObject>();
    private DestructibleMeshComponent currentComponent;
    private float timer;
    private bool isDestructionActive = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (meshSpawner != null)
        {
            meshSpawner.OnDestructibleMeshCreated.AddListener(SetupDestructibleComponent);
        }
        else
        {
            Debug.LogError("Mesh Spawner is not assigned in destructibleGlobalMeshManager! Please assign it in the Inspector.");
        }
        
        // FAILSAFE: Check if the mesh was already created before we started listening
        var existingMesh = FindFirstObjectByType<DestructibleMeshComponent>();
        if (existingMesh != null)
        {
            SetupDestructibleComponent(existingMesh);
        }
        
        timer = destructionInterval;
    }

    void Update()
    {
        // Only run if active and we have segments to destroy
        if (isDestructionActive && segments.Count > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                DestroyRandomSegment();
                timer = destructionInterval;
            }
        }
    }

    public void SetupDestructibleComponent(DestructibleMeshComponent component)
    {
        currentComponent = component;
        segments.Clear();

        foreach (Transform child in component.transform)
        {
            segments.Add(child.gameObject);
        }

        foreach (var item in segments)
        {
            // check to avoid adding duplicate colliders
            if (item.GetComponent<MeshCollider>() == null)
            {
                item.AddComponent<MeshCollider>();
            }
        }
    }

    public void DestroyMeshSegment(GameObject segment)
    {
        if(segments.Remove(segment) && currentComponent.ReservedSegment != segment)
        {
            currentComponent.DestroySegment(segment);
        }
    }

    private void DestroyRandomSegment()
    {
        if (segments.Count == 0) return;

        // Get a random index
        int index = Random.Range(0, segments.Count);
        GameObject segmentToDestroy = segments[index];

        // Use the existing method to destroy it properly
        DestroyMeshSegment(segmentToDestroy);
    }

    public void SetDestructionActive(bool active)
    {
        isDestructionActive = active;
        if (active)
        {
            timer = destructionInterval; // Reset timer when activating
        }
    }
}