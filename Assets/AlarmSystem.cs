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

    void Start()
    {
        // 1. Auto-find components if not assigned
        if (alarmCanvasGroup == null) alarmCanvasGroup = GetComponent<CanvasGroup>();
        if (alertText == null) alertText = GetComponentInChildren<TextMeshProUGUI>();

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

        float timer = 0f;

        // Loop for the duration
        while (timer < duration)
        {
            timer += Time.deltaTime;

            // Math to create a sharp flickering effect (0 to 1)
            float flicker = Mathf.PingPong(Time.time * flickerSpeed, 1.0f);
            
            // Keep it mostly visible, don't flicker to fully invisible (0.2 to 1.0)
            alarmCanvasGroup.alpha = Mathf.Lerp(0.2f, 1.0f, flicker);

            yield return null;
        }

        alarmCanvasGroup.alpha = 0f;
        alarmCanvasGroup.gameObject.SetActive(false); 
    }
}