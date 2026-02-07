using UnityEngine;
using Meta.XR.MRUtilityKit;
// Force Recompile
using System.Collections.Generic;
using System.Collections; // Needed for Coroutines

public class destructibleGlobalMeshManager : MonoBehaviour
{
    public DestructibleGlobalMeshSpawner meshSpawner;
    public float destructionInterval = 7.0f;
    
    private List<GameObject> segments = new List<GameObject>();
    private Dictionary<GameObject, GameObject> hitboxToSegmentMap = new Dictionary<GameObject, GameObject>();
    
    [Header("Visuals")]
    public GameObject repairBoxPrefab; 
    public float repairBoxSpawnOffset = 0.0f; 

    [Header("Enemy Spawning")]
    public GameObject enemyPrefab; 
    public float spawnBehindWallDistance = 4.0f; // Increased to 4m for better visibility test 
    public float spawnBehindWallDistance = 4.0f; // Increased to 4m for better visibility test 
    
    // Controlled by GameManager: Range of enemies per breach
    public int minEnemiesPerBreach = 1; 
    public int maxEnemiesPerBreach = 1; 

    [Header("Audio")]
    public AudioClip[] wallCrackSounds;
    public AudioClip wallHealSound;

    private DestructibleMeshComponent currentComponent;
    private float timer;
    private bool isDestructionActive = false;

    // Events to notify GameManager
    public System.Action OnEnemySpawned;
    public System.Action OnSegmentBroken;

    public int GetBrokenWallCount()
    {
        return hitboxToSegmentMap.Count;
    }

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

            // Play Random Crack Sound
            if (wallCrackSounds != null && wallCrackSounds.Length > 0)
            {
                AudioClip clip = wallCrackSounds[Random.Range(0, wallCrackSounds.Length)];
                if (clip != null)
                {
                    Renderer r = segment.GetComponent<Renderer>();
                    Vector3 playPos = r ? r.bounds.center : segment.transform.position;
                    AudioSource.PlayClipAtPoint(clip, playPos);
                }
            }

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

            // Notify GameManager that a segment broke (used for wave progress)
            OnSegmentBroken?.Invoke();

            // --- DELAY LOGIC START ---
            // Hide the hitbox immediately so it doesn't block the enemy or the view
            hitbox.SetActive(false);
            
            // Calculate random delay (8 to 15 seconds)
            float delay = Random.Range(8.0f, 15.0f);
            StartCoroutine(ActivateHitboxRoutine(hitbox, delay));
            // -------------------------

            // --- 2. SPAWN ENEMIES ---
            if (enemyPrefab != null)
            {
                Vector3 wallCenter = (segRenderer != null) ? segRenderer.bounds.center : segment.transform.position;
                Vector3 playerPos = Camera.main ? Camera.main.transform.position : Vector3.zero;

                // Loop for multiple enemies (Randomized count)
                int enemiesToSpawn = Random.Range(minEnemiesPerBreach, maxEnemiesPerBreach + 1); // +1 because Random.Range int is exclusive at max

                for (int i = 0; i < enemiesToSpawn; i++)
                {
                    // ROBUST LOGIC: Calculate direction from Player to Wall
                    Vector3 outwardDir = (wallCenter - playerPos);
                    
                    // Flatten Y
                    outwardDir.y = 0;
                    outwardDir.Normalize();
                    
                    if(outwardDir == Vector3.zero) outwardDir = Vector3.forward;

                    // Base Spawn Pos
                    Vector3 spawnPos = wallCenter + (outwardDir * spawnBehindWallDistance);

                    // Add Random Offset for multiple enemies (or even single ones for variety)
                    // We scramble X/Z relative to the world, simpler than local right vector calculation for now
                    if (enemiesToSpawn > 1 || i > 0)
                    {
                        float entropy = 1.5f; // Spread amount
                        Vector3 offset = new Vector3(Random.Range(-entropy, entropy), 0, Random.Range(-entropy, entropy));
                        spawnPos += offset;
                    }
                    
                    // Force height
                    spawnPos.y = wallCenter.y;
                    
                    // Spawn enemy looking AT the wall
                    GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.LookRotation(-outwardDir));

                    enemy enemyScript = newEnemy.GetComponent<enemy>();
                    if (enemyScript != null)
                    {
                        Vector3 roomEntryPos = wallCenter;
                        enemyScript.InitializeBreach(roomEntryPos);
                    }

                    // Notify GameManager
                    OnEnemySpawned?.Invoke();
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

            if (wallHealSound != null)
            {
                Renderer r = segment.GetComponent<Renderer>();
                Vector3 playPos = r ? r.bounds.center : segment.transform.position;
                AudioSource.PlayClipAtPoint(wallHealSound, playPos);
            }

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