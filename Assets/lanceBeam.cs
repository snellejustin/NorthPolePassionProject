using UnityEngine;

public class lanceBeam : MonoBehaviour
{
    public LayerMask layerMask;
    public OVRInput.RawButton shootingButton;
    public LineRenderer linePrefab;
    public GameObject heatImpactPrefab;
    public Transform shootingPoint;
    public float maxLineDistance = 10;
    public AudioSource audioSource;
    public AudioClip shootingAudioClip;

    private LineRenderer currentLine;
    private Vector3 lastHitPosition;
    public float spawnDistance = 0.05f; // Adjust this for denser/sparser trail

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(shootingButton))
        {
            StartShooting();
        }
        else if (OVRInput.GetUp(shootingButton))
        {
            StopShooting();
        }

        if (currentLine != null)
        {
            UpdateBeam();
        }
    }

    private void StartShooting()
    {
        audioSource.Stop(); // Ensure it resets if pressed quickly
        audioSource.PlayOneShot(shootingAudioClip);
        
        if (currentLine == null)
        {
            currentLine = Instantiate(linePrefab);
            currentLine.positionCount = 2;
        }
        
        // Reset last hit position so the first hit always spawns
        lastHitPosition = Vector3.zero; 
    }

    private void StopShooting()
    {
        audioSource.Stop();
        
        if (currentLine != null)
        {
            Destroy(currentLine.gameObject);
            currentLine = null;
        }
    }

    public void UpdateBeam()
    {
        currentLine.SetPosition(0, shootingPoint.position);

        Ray ray = new Ray(shootingPoint.position, shootingPoint.forward);
        // Standard Raycast is fine now because Hitboxes are solid colliders
        bool hasHit = Physics.Raycast(ray, out RaycastHit hit, maxLineDistance, layerMask);

        Vector3 endPoint;

        if (hasHit)
        {
            endPoint = hit.point;

            enemy enemyScript = hit.transform.GetComponentInParent<enemy>();
            if (enemyScript)
            {
                hit.collider.enabled = false;
                enemyScript.Kill();
            }
            // Check for Wall Repair Hitbox
            // Check for Wall Repair Hitbox
            else if (FindFirstObjectByType<destructibleGlobalMeshManager>()?.IsHitbox(hit.collider.gameObject) == true)
            {
                var meshManager = FindFirstObjectByType<destructibleGlobalMeshManager>();
                meshManager.RepairMeshSegment(hit.collider.gameObject);
            }
            // Check if we moved enough to spawn a new "weld" point
            else if (Vector3.Distance(hit.point, lastHitPosition) > spawnDistance)
            {
                Quaternion rotation = Quaternion.LookRotation(-hit.normal);
                GameObject heatImpact = Instantiate(heatImpactPrefab, hit.point, rotation);
                Destroy(heatImpact, 1f);
                
                lastHitPosition = hit.point;
            }
        }
        else
        {
            endPoint = shootingPoint.position + shootingPoint.forward * maxLineDistance;
            lastHitPosition = Vector3.zero; // Reset if we miss, so next hit spawns immediately
        }
        
        currentLine.SetPosition(1, endPoint);
    }
}
