using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;

public class AlarmSystem : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup alarmCanvasGroup; 
    public TextMeshProUGUI alertText;   

    [Header("Settings")]
    public float duration = 5.0f;        
    public float flickerSpeed = 5.0f;    
    public string startMessage = "BREACH DETECTED";

    [Header("Audio")]
    public AudioClip alarmSound;
    public AudioSource audioSource;

    void Start()
    {
        if (alarmCanvasGroup == null) alarmCanvasGroup = GetComponent<CanvasGroup>();
        if (alertText == null) alertText = GetComponentInChildren<TextMeshProUGUI>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (alarmCanvasGroup != null)
        {
            alarmCanvasGroup.alpha = 0f; 
        }
    }
    public void TriggerAlarm(string message)
    {
        Debug.Log($"[AlarmSystem] Triggering Alarm with message: '{message}'");
        TriggerAlarm(message, 3); 
    }

    public void TriggerAlarm(string message, int flickerCount)
    {
        if (alarmCanvasGroup != null) alarmCanvasGroup.gameObject.SetActive(true);
        
        StopAllCoroutines(); 
        
        float cycleDuration = 2.0f / flickerSpeed;
        float totalDuration = cycleDuration * flickerCount;
        
        StartCoroutine(AlarmRoutine(message, totalDuration));
    }

    private IEnumerator AlarmRoutine(string message, float customDuration)
    {
        if (alarmCanvasGroup == null)
        {
            Debug.LogError("[AlarmSystem] No CanvasGroup found!");
            yield break;
        }

        if (alertText != null) alertText.text = message;

        alarmCanvasGroup.alpha = 0f;

        float elapsed = 0f;
        float cycleDuration = 2.0f / flickerSpeed; 

        while (elapsed < customDuration)
        {
            if (audioSource != null && alarmSound != null)
            {
                audioSource.Stop(); 
                audioSource.clip = alarmSound;
                audioSource.Play();
            }

            float cycleTimer = 0f;
            while (cycleTimer < cycleDuration && elapsed < duration)
            {
                cycleTimer += Time.deltaTime;
                elapsed += Time.deltaTime;

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