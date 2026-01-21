using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

public class destructibleGlobalMeshManager : MonoBehaviour
{
    public DestructibleGlobalMeshSpawner meshSpawner;
    public float destructionInterval = 2.0f; // Time in seconds between destruction
    
    private List<GameObject> segments = new List<GameObject>();
    // Map Hitbox (Key) -> Original Segment (Value)
    private Dictionary<GameObject, GameObject> hitboxToSegmentMap = new Dictionary<GameObject, GameObject>();
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
        hitboxToSegmentMap.Clear();

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
            // 1. Disable the actual wall segment (effectively deleting it from view/physics)
            segment.SetActive(false);

            // 2. Create a temporary hitbox in its place
            GameObject hitbox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hitbox.name = "RepairHitbox";
            
            // Position it at the center of the segment's bounds
            Renderer segRenderer = segment.GetComponent<Renderer>();
            if (segRenderer)
            {
                hitbox.transform.position = segRenderer.bounds.center;
                hitbox.transform.rotation = segment.transform.rotation;
                hitbox.transform.localScale = Vector3.one * 0.5f; // Small target
            }
            else
            {
                hitbox.transform.position = segment.transform.position;
            }

            // Make hitbox visible so the user can see where to aim
            hitbox.GetComponent<Renderer>().enabled = true; 
            
            // 3. Register it
            hitboxToSegmentMap.Add(hitbox, segment);
        }
    }

    public void RepairMeshSegment(GameObject hitbox)
    {
        if (hitboxToSegmentMap.TryGetValue(hitbox, out GameObject segment))
        {
            // 1. Re-enable the original segment
            segment.SetActive(true);
            segments.Add(segment);

            // 2. Remove the hitbox
            hitboxToSegmentMap.Remove(hitbox);
            Destroy(hitbox);
        }
    }

    public bool IsHitbox(GameObject obj)
    {
        return hitboxToSegmentMap.ContainsKey(obj);
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