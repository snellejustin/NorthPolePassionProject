using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject startCanvas;
    public GameObject gameOverCanvas;
    public int maxEnemies = 15;

    public GameObject lanceObject;
    public GameObject xrRayInteractorObject;
    public destructibleGlobalMeshManager destructionManager;
    
    [Header("Effects")]
    public AlarmSystem alarmSystem; // DRAG YOUR ALARM HUD HERE

    [Header("Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    public AudioClip buttonSound;

    // Nieuwe variabele om bij te houden of het spel bezig is
    private bool isGameActive = false;

    void Start()
    {
        if (musicSource && menuMusic)
        {
            musicSource.clip = menuMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
        ShowStartScreen();
    }

    void Update()
    {
        // Check game over conditie alleen als het spel actief is
        if (isGameActive)
        {
            // Tel actieve enemies (dit werkt nog steeds hetzelfde)
            // Let op: Zorg dat je enemy.cs script de class naam 'enemy' heeft (kleine letter e zoals in je eerdere code)
            int enemyCount = FindObjectsByType<enemy>(FindObjectsSortMode.None).Length;
            
            if (enemyCount >= maxEnemies)
            {
                GameOver();
            }
        }
    }

    public void StartGame()
    {
        PlayButtonSound();

        // Fade out menu music, then start game music
        StartCoroutine(SwitchMusic(gameplayMusic, 1.0f));

        startCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
        
        isGameActive = true; // We starten het spel
        
        if(lanceObject) lanceObject.SetActive(true);
        if(xrRayInteractorObject) xrRayInteractorObject.SetActive(false);
        
        // Dit start nu de destructie EN de enemy spawning
        if(destructionManager) destructionManager.SetDestructionActive(true);

        // TRIGGER THE ALARM
        if(alarmSystem) alarmSystem.TriggerAlarm("BREACH DETECTED");
    }

    public void GameOver()
    {
        isGameActive = false; // We stoppen het spel
        gameOverCanvas.SetActive(true);

        if(lanceObject) lanceObject.SetActive(false);
        if(xrRayInteractorObject) xrRayInteractorObject.SetActive(true);
        
        if(destructionManager) destructionManager.SetDestructionActive(false);
    }

    public void RestartGame()
    {
        PlayButtonSound();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        PlayButtonSound();
        Application.Quit();
    }

    private void PlayButtonSound()
    {
        if (sfxSource && buttonSound)
        {
            sfxSource.PlayOneShot(buttonSound);
        }
    }

    private System.Collections.IEnumerator SwitchMusic(AudioClip newClip, float fadeDuration)
    {
        if (musicSource == null) yield break;

        float startVolume = musicSource.volume;

        // Fade Out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }
        
        musicSource.volume = 0;
        musicSource.Stop();

        // Swap and Play
        if (newClip != null)
        {
            musicSource.clip = newClip;
            musicSource.Play();
            
            // Fade In (Optional, but smoother)
            // We reuse the original volume target
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(0, startVolume, t / fadeDuration);
                yield return null;
            }
            musicSource.volume = startVolume;
        }
    }

    private void ShowStartScreen()
    {
        startCanvas.SetActive(true);
        gameOverCanvas.SetActive(false);
        
        isGameActive = false;

        if(lanceObject) lanceObject.SetActive(false);
        if(xrRayInteractorObject) xrRayInteractorObject.SetActive(true);
        if(destructionManager) destructionManager.SetDestructionActive(false);
    }
}