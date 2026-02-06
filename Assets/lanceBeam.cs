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
    public AudioClip errorSound;
    
    [Header("Welding UI")]
    public RepairProgressUI repairHUD; 

    [Header("Heat & Cooldown")]
    public CooldownUI cooldownHUD;
    public float maxHeatTime = 5f; 
    public float coolingRateMultiplier = 1f; 
    
    private float currentHeat = 0f; 
    private bool isOverheated = false;

    private LineRenderer currentLine;
    private Vector3 lastHitPosition;

    public float spawnDistance = 0.05f; // Adjust this for denser/sparser trail
    
    // Welding Logic
    private float currentWeldProgress = 0f;
    private float weldDuration = 2.0f;
    private GameObject currentWeldTarget;

    void Start()
    {
        if (repairHUD) repairHUD.Hide();
        if (cooldownHUD) cooldownHUD.SetProgress(0, false);
    }

    void Update()
    {
        HandleHeat();

        bool canShoot = !isOverheated && currentHeat < 1.0f;

        if (canShoot && OVRInput.GetDown(shootingButton))
        {
            StartShooting();
        }
        else if (OVRInput.GetUp(shootingButton) || (isOverheated && currentLine != null))
        {
            StopShooting();
        }

        if (isOverheated && OVRInput.GetDown(shootingButton))
        {
             if (audioSource && errorSound) audioSource.PlayOneShot(errorSound);
        }

        if (currentLine != null)
        {
            UpdateBeam();
        }
    }

    private void HandleHeat()
    {
        if (currentLine != null && !isOverheated)
        {
            // Heating up
            currentHeat += Time.deltaTime / maxHeatTime;
            if (currentHeat >= 1f)
            {
                currentHeat = 1f;
                isOverheated = true;
                if (audioSource && errorSound) audioSource.PlayOneShot(errorSound);
                // StopShooting will be called in Update loop next check
            }
        }
        else
        {
            // Cooling down
            currentHeat -= (Time.deltaTime / maxHeatTime) * coolingRateMultiplier;
            if (currentHeat <= 0f)
            {
                currentHeat = 0f;
                isOverheated = false;
            }
        }

        if (cooldownHUD)
        {
            cooldownHUD.SetProgress(currentHeat, isOverheated);
        }
    }

    private void StartShooting()
    {
        if (audioSource)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(shootingAudioClip);
        }
        
        if (currentLine == null)
        {
            currentLine = Instantiate(linePrefab);
            currentLine.positionCount = 2;
        }
        
        lastHitPosition = Vector3.zero; 
    }

    private void StopShooting()
    {
        if (audioSource) audioSource.Stop();
        
        if (currentLine != null)
        {
            Destroy(currentLine.gameObject);
            currentLine = null;
        }
        
        ResetWeldProgress();
    }

    private void ResetWeldProgress()
    {
        if (repairHUD)
        {
            repairHUD.SetProgress(0f);
            repairHUD.Hide();
        }
        currentWeldTarget = null;
        currentWeldProgress = 0f;
    }

    public void UpdateBeam()
    {
        currentLine.SetPosition(0, shootingPoint.position);

        Ray ray = new Ray(shootingPoint.position, shootingPoint.forward);
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
                ResetWeldProgress();
            }
            else 
            {
                var meshManager = FindFirstObjectByType<destructibleGlobalMeshManager>();
                GameObject hitboxRoot = meshManager != null ? meshManager.GetHitboxFromObject(hit.collider.gameObject) : null;

                if (hitboxRoot != null)
                {
                    if (currentWeldTarget != hitboxRoot)
                    {
                        currentWeldTarget = hitboxRoot;
                        currentWeldProgress = 0f;
                    }

                    currentWeldProgress += Time.deltaTime;
                    float progressPercent = Mathf.Clamp01(currentWeldProgress / weldDuration);
                    
                    if (repairHUD)
                    {
                        repairHUD.Show();
                        repairHUD.SetProgress(progressPercent);
                    }

                    if (currentWeldProgress >= weldDuration)
                    {
                        meshManager.RepairMeshSegment(currentWeldTarget);
                        ResetWeldProgress();
                    }
                }
                else
                {
                    ResetWeldProgress();

                    if (Vector3.Distance(hit.point, lastHitPosition) > spawnDistance)
                    {
                        Quaternion rotation = Quaternion.LookRotation(-hit.normal);
                        
                        // FIX: Til de impact 2cm van de muur af om Z-fighting met de Occlusion Mesh te voorkomen
                        Vector3 spawnPos = hit.point + (hit.normal * 0.02f);
                        
                        GameObject heatImpact = Instantiate(heatImpactPrefab, spawnPos, rotation);
                        Destroy(heatImpact, 1f);
                        lastHitPosition = hit.point;
                    }
                }
            }
        }
        else
        {
            endPoint = shootingPoint.position + shootingPoint.forward * maxLineDistance;
            lastHitPosition = Vector3.zero;
            ResetWeldProgress();
        }
        
        currentLine.SetPosition(1, endPoint);
    }
}