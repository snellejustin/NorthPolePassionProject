using UnityEngine;

public class FrostEffectController : MonoBehaviour
{
    public enum ControlMode { EnemyCount, TimeLoop, ManualSlider }

    [Header("Setup")]
    public Renderer frostQuadRenderer; 
    public string intensityPropertyName = "_VignetteIntensity"; 

    [Header("Testing")]
    public ControlMode controlMode = ControlMode.EnemyCount;
    [Range(0f, 1.7f)]
    public float manualIntensity = 0.5f;
    public bool showDebugLogs = false;

    [Header("Game Logic")]
    public float maxEnemiesForFullFreeze = 15f; 
    public float maxFrostIntensity = 2f; // New Max Cap
    
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

        // Reset and Verify Property
        if (_targetMaterial != null) 
        {
            if (_targetMaterial.HasProperty(intensityPropertyName))
            {
                _targetMaterial.SetFloat(intensityPropertyName, 0f);
            }
            else
            {
                Debug.LogError($"[FrostEffectController] Property '{intensityPropertyName}' NOT found on material '{_targetMaterial.name}'! " +
                               $"Please check the 'Reference' name in Shader Graph (Blackboard > VignetteIntensity > ReferenceName). " +
                               $"Try removing the underscore in the Inspector.");
            }
        }
    }

    void Update()
    {
        if (_targetMaterial == null) return;

        switch (controlMode)
        {
            case ControlMode.EnemyCount:
                int count = ActiveEnemyCount;
                
                // LINEAR LOGIC: (count / max) * 1.7
                // 0 enemies = 0.0
                // 15 enemies = 1.7
                float fraction = Mathf.Clamp01((float)count / maxEnemiesForFullFreeze);
                targetIntensity = fraction * maxFrostIntensity;
                
                if (showDebugLogs && Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[Frost] Enemies: {count} | Target: {targetIntensity:F2} | Current: {currentIntensity:F2}");
                }
                break;

            case ControlMode.TimeLoop:
                // Gaat van 0 naar 1 en terug
                targetIntensity = Mathf.Lerp(0f, 1.0f, Mathf.PingPong(Time.time * 0.5f, 1.0f));
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