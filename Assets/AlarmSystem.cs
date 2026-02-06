using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;

public class AlarmSystem : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup alarmCanvasGroup; // We use this to fade/flicker everything at once
    public TextMeshProUGUI alertText;   

    [Header("Settings")]
    public float duration = 5.0f;        // How long the alarm lasts
    public float flickerSpeed = 5.0f;    // How fast it flashes
    public string startMessage = "BREACH DETECTED";

    [Header("Audio")]
    public AudioClip alarmSound;
    public AudioSource audioSource;

    void Start()
    {
        // 1. Auto-find components if not assigned
        if (alarmCanvasGroup == null) alarmCanvasGroup = GetComponent<CanvasGroup>();
        if (alertText == null) alertText = GetComponentInChildren<TextMeshProUGUI>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // 2. Ensure it starts hidden (BUT ACTIVE)
        if (alarmCanvasGroup != null)
        {
            alarmCanvasGroup.alpha = 0f; 
            // DO NOT set gameObject.SetActive(false) here, or the script stops running!
        }
    }

    // Call this function later for waves: TriggerAlarm("WAVE 2 INCOMING");
    public void TriggerAlarm(string message)
    {
        // Ensure object is on before starting coroutine
        if (alarmCanvasGroup != null) alarmCanvasGroup.gameObject.SetActive(true);
        
        StopAllCoroutines(); 
        StartCoroutine(AlarmRoutine(message));
    }

    private IEnumerator AlarmRoutine(string message)
    {
        if (alarmCanvasGroup == null)
        {
            Debug.LogError("[AlarmSystem] No CanvasGroup found!");
            yield break;
        }

        if (alertText != null) alertText.text = message;

        // Ensure we start invisible
        alarmCanvasGroup.alpha = 0f;

        float elapsed = 0f;
        float cycleDuration = 2.0f / flickerSpeed; // Time for one full 0->1->0 cycle

        while (elapsed < duration)
        {
            // Play sound at start of pulse/cycle
            if (audioSource != null && alarmSound != null)
            {
                audioSource.PlayOneShot(alarmSound);
            }

            // Perform one visual cycle
            float cycleTimer = 0f;
            while (cycleTimer < cycleDuration && elapsed < duration)
            {
                cycleTimer += Time.deltaTime;
                elapsed += Time.deltaTime;

                // Sync flicker manually: 0 -> 1 -> 0
                float t = cycleTimer / cycleDuration;
                float flicker = Mathf.PingPong(t * 2.0f, 1.0f);

                alarmCanvasGroup.alpha = Mathf.Lerp(0.2f, 1.0f, flicker);
                yield return null;
            }
        }

        alarmCanvasGroup.alpha = 0f;
        alarmCanvasGroup.gameObject.SetActive(false); 
    }
}