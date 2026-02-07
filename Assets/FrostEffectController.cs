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
    public float maxEnemiesForFullFreeze = 8f; 
    public float maxFrostIntensity = 2f; 
    
    [Header("Smoothness Settings")]
    public float smoothTime = 0.5f; 

    public static int ActiveEnemyCount = 0;

    private float targetIntensity = 0f;
    private float currentIntensity = 0f;
    private float currentVelocity = 0f;
    private Material _targetMaterial; 

    void Start()
    {
        if (frostQuadRenderer == null)
        {
            Transform child = transform.Find("FrostVisor");
            if (child != null) frostQuadRenderer = child.GetComponent<Renderer>();
            
            if (frostQuadRenderer == null)
            {
                GameObject obj = GameObject.Find("FrostVisor");
                if (obj != null) frostQuadRenderer = obj.GetComponent<Renderer>();
            }
        }

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
                int count = 0;
                GameManager gm = FindFirstObjectByType<GameManager>();
                if (gm != null) 
                {
                    count = gm.CurrentStrikePoints;
                }
                else
                {
                    count = ActiveEnemyCount; 
                }
                
                float fraction = Mathf.Clamp01((float)count / maxEnemiesForFullFreeze);
                targetIntensity = fraction * maxFrostIntensity;
                
                if (showDebugLogs && Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[Frost] Strike Points: {count} | Target: {targetIntensity:F2} | Current: {currentIntensity:F2}");
                }
                break;

            case ControlMode.TimeLoop:
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