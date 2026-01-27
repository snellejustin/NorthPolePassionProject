using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;
using System.Collections; // Needed for Coroutines

public class destructibleGlobalMeshManager : MonoBehaviour
{
    public DestructibleGlobalMeshSpawner meshSpawner;
    public float destructionInterval = 2.0f;
    
    private List<GameObject> segments = new List<GameObject>();
    private Dictionary<GameObject, GameObject> hitboxToSegmentMap = new Dictionary<GameObject, GameObject>();
    
    [Header("Visuals")]
    public GameObject repairBoxPrefab; 
    public float repairBoxSpawnOffset = 0.0f; 

    [Header("Enemy Spawning")]
    public GameObject enemyPrefab; 
    public float spawnBehindWallDistance = 4.0f; // Increased to 4m for better visibility test 

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

        // Fix Occlusion: Hide the original static walls so we can see through the holes
        DisableOriginalWallVisuals();
    }

    private void DisableOriginalWallVisuals()
    {
        if (MRUK.Instance == null) return;
        var room = MRUK.Instance.GetCurrentRoom();
        if (room == null) return;

        foreach (var anchor in room.WallAnchors)
        {
            // Disable MeshRenderers on the original wall anchors (and their children)
            // This ensures the solid "real" wall doesn't block the view of the enemy behind the "destructible" wall
            var renderers = anchor.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in renderers) r.enabled = false;
        }
    }

    public void DestroyMeshSegment(GameObject segment)
    {
        if(segments.Remove(segment) && currentComponent.ReservedSegment != segment)
        {
            segment.SetActive(false);

            // --- 1. SPAWN REPAIR HITBOX ---
            GameObject hitbox;
            if (repairBoxPrefab != null)
            {
                hitbox = Instantiate(repairBoxPrefab);
            }
            else
            {
                hitbox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hitbox.GetComponent<Renderer>().material.color = new Color(1, 0.5f, 0); 
            }
            hitbox.name = "RepairHitbox";
            
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

            // --- DELAY LOGIC START ---
            // Hide the hitbox immediately so it doesn't block the enemy or the view
            hitbox.SetActive(false);
            
            // Calculate random delay (8 to 15 seconds)
            float delay = Random.Range(8.0f, 15.0f);
            StartCoroutine(ActivateHitboxRoutine(hitbox, delay));
            // -------------------------

            // --- 2. SPAWN ENEMY ---
            if (enemyPrefab != null)
            {
                Vector3 wallCenter = (segRenderer != null) ? segRenderer.bounds.center : segment.transform.position;
                Vector3 playerPos = Camera.main ? Camera.main.transform.position : Vector3.zero;

                // ROBUST LOGIC: Calculate direction from Player to Wall
                // This gives us the "Outward" vector regardless of debris rotation.
                Vector3 outwardDir = (wallCenter - playerPos);
                
                // Flatten Y to ensure we don't shoot into the sky or floor
                outwardDir.y = 0;
                outwardDir.Normalize();
                
                // If for some reason player is exactly on top of wall (zero vector), default to Z forward
                if(outwardDir == Vector3.zero) outwardDir = Vector3.forward;

                // Spawn 4 meters "Further out" from the wall center
                Vector3 spawnPos = wallCenter + (outwardDir * spawnBehindWallDistance);
                
                // Ensure the enemy stays at the same height as the hole (or floor aligned?)
                // User complained about "down under", so let's stick to the hole's Y level.
                // But wait, if hole is high, enemy floats. 
                // Let's force spawn Y to be the same as wallCenter Y (hole height).
                 spawnPos.y = wallCenter.y;
                
                // Spawn enemy looking AT the wall (opposite to outward direction)
                GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.LookRotation(-outwardDir));

                enemy enemyScript = newEnemy.GetComponent<enemy>();
                if (enemyScript != null)
                {
                    // Use the outward direction to calculate the entry point (just slightly inside the room/hole)
                    // We move from Outside -> WallCenter -> Inside
                    // Actually, WallCenter IS the hole. 
                    // Let's set the target to the wall center itself.
                    Vector3 roomEntryPos = wallCenter;
                    enemyScript.InitializeBreach(roomEntryPos);
                }
            }
        }
    }

    // This makes the box appear after the enemy has (hopefully) fallen
    IEnumerator ActivateHitboxRoutine(GameObject hitbox, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Check if hitbox still exists (game might have ended)
        if (hitbox != null)
        {
            hitbox.SetActive(true);
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

    public GameObject GetHitboxFromObject(GameObject obj)
    {
        if (obj == null) return null;
        Transform current = obj.transform;
        while (current != null)
        {
            if (hitboxToSegmentMap.ContainsKey(current.gameObject)) return current.gameObject;
            current = current.parent;
        }
        return null;
    }

    public bool IsHitbox(GameObject obj)
    {
        return GetHitboxFromObject(obj) != null;
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