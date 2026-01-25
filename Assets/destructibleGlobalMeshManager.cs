using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

public class destructibleGlobalMeshManager : MonoBehaviour
{
    public DestructibleGlobalMeshSpawner meshSpawner;
    public float destructionInterval = 2.0f;
    
    private List<GameObject> segments = new List<GameObject>();
    private Dictionary<GameObject, GameObject> hitboxToSegmentMap = new Dictionary<GameObject, GameObject>();
    
    [Header("Visuals")]
    public GameObject repairBoxPrefab; // DRAG YOUR ORANGE CUBE HERE
    public float repairBoxSpawnOffset = 0.0f; 

    private DestructibleMeshComponent currentComponent;
    private float timer;
    private bool isDestructionActive = false;

    void Start()
    {
        if (meshSpawner != null) meshSpawner.OnDestructibleMeshCreated.AddListener(SetupDestructibleComponent);
        
        var existingMesh = FindFirstObjectByType<DestructibleMeshComponent>();
        if (existingMesh != null) SetupDestructibleComponent(existingMesh);
        
        timer = destructionInterval;
    }

    void Update()
    {
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

        foreach (Transform child in component.transform) segments.Add(child.gameObject);
        foreach (var item in segments)
        {
            if (item.GetComponent<MeshCollider>() == null) item.AddComponent<MeshCollider>();
        }
    }

    public void DestroyMeshSegment(GameObject segment)
    {
        if(segments.Remove(segment) && currentComponent.ReservedSegment != segment)
        {
            segment.SetActive(false);

            // --- PREFAB LOGIC (Back to Basics) ---
            GameObject hitbox;
            
            if (repairBoxPrefab != null)
            {
                hitbox = Instantiate(repairBoxPrefab);
            }
            else
            {
                // Backup just in case the slot is empty
                hitbox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hitbox.GetComponent<Renderer>().material.color = new Color(1, 0.5f, 0); // Orange
            }

            hitbox.name = "RepairHitbox";
            
            // Position Logic
            Renderer segRenderer = segment.GetComponent<Renderer>();
            if (segRenderer)
            {
                hitbox.transform.rotation = segment.transform.rotation;
                hitbox.transform.position = segRenderer.bounds.center + (segment.transform.forward * repairBoxSpawnOffset); 
            }
            else
            {
                hitbox.transform.position = segment.transform.position;
            }

            hitboxToSegmentMap.Add(hitbox, segment);
            // -------------------------------------
        }
    }

    public void RepairMeshSegment(GameObject hitbox)
    {
        if (hitboxToSegmentMap.TryGetValue(hitbox, out GameObject segment))
        {
            segment.SetActive(true);
            segments.Add(segment);
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
        int index = Random.Range(0, segments.Count);
        DestroyMeshSegment(segments[index]);
    }

    public void SetDestructionActive(bool active)
    {
        isDestructionActive = active;
        if (active) timer = destructionInterval;
    }
}