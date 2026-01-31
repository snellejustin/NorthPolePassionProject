using UnityEngine;

public class FrostEffectController : MonoBehaviour
{
    public enum ControlMode { EnemyCount, TimeLoop, ManualSlider }

    [Header("Setup")]
    public Renderer frostQuadRenderer; 
    public string intensityPropertyName = "_VignetteIntensity"; 

    [Header("Testing")]
    public ControlMode controlMode = ControlMode.EnemyCount;
    [Range(0f, 1f)]
    public float manualIntensity = 0.5f;
    public bool showDebugLogs = false;

    [Header("Game Logic")]
    public float maxEnemiesForFullFreeze = 15f; 
    public float minFrostIntensity = 0.3f; // Base frost level (0.3 at 0 enemies)
    public float maxFrostIntensity = 1.5f; // Max frost level (1.5 at 15 enemies)
    
    [Header("Smoothness Settings")]
    public float smoothTime = 0.5f; // Time (seconds) to reach the target value. Higher = Smoother/Slower.

    // STATIC COUNTER
    public static int ActiveEnemyCount = 0;

    private float targetIntensity = 0f;
    private float currentIntensity = 0f;
    private float currentVelocity = 0f; // Helper for SmoothDamp
    private Material _targetMaterial; 

    void Start()
    {
        // 1. Auto-find renderer if not assigned
        if (frostQuadRenderer == null)
        {
            // Try to find child named FrostVisor
            Transform child = transform.Find("FrostVisor"); // If it's a child of this object
            if (child != null) frostQuadRenderer = child.GetComponent<Renderer>();
            
            // Or try finding it in the scene (slower but robust)
            if (frostQuadRenderer == null)
            {
                GameObject obj = GameObject.Find("FrostVisor");
                if (obj != null) frostQuadRenderer = obj.GetComponent<Renderer>();
            }
        }

        // 2. Get Material Instance
        if (frostQuadRenderer != null)
        {
            _targetMaterial = frostQuadRenderer.material;
            frostQuadRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            frostQuadRenderer.receiveShadows = false;
        }
        else
        {
            Debug.LogError("[FrostEffectController] CRITICAL: No FrostVisor Renderer found!");
        }
        if (_targetMaterial != null) 
        {
            currentIntensity = minFrostIntensity;
            _targetMaterial.SetFloat(intensityPropertyName, minFrostIntensity);
        }
    }

    void Update()
    {
        if (_targetMaterial == null) return;

        switch (controlMode)
        {
            case ControlMode.EnemyCount:
                int count = ActiveEnemyCount;
                float linearFraction = Mathf.Clamp01((float)count / maxEnemiesForFullFreeze);
                
                // Wortel curve voor snellere start
                float curve = Mathf.Pow(linearFraction, 0.5f); 
                targetIntensity = Mathf.Lerp(minFrostIntensity, maxFrostIntensity, curve);
                
                if (showDebugLogs && Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[Frost] Enemies: {count} | Target: {targetIntensity:F2} | Current: {currentIntensity:F2}");
                }
                break;

            case ControlMode.TimeLoop:
                // Gaat van Min naar 1 en terug
                targetIntensity = Mathf.Lerp(minFrostIntensity, 1.0f, Mathf.PingPong(Time.time * 0.5f, 1.0f));
                break;

            case ControlMode.ManualSlider:
                targetIntensity = manualIntensity;
                break;
        }
        currentIntensity = Mathf.SmoothDamp(currentIntensity, targetIntensity, ref currentVelocity, smoothTime);
        _targetMaterial.SetFloat(intensityPropertyName, currentIntensity);
    }
    
    void OnDisable()
    {
         if (_targetMaterial != null) _targetMaterial.SetFloat(intensityPropertyName, 0f);
    }
}