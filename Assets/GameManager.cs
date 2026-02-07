using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public GameObject startCanvas;
    public GameObject gameOverCanvas;
    
    [Header("Game Settings")]
    public int strikePointLimit = 15;
    public float restorationDuration = 10.0f; 
    [Header("Wave Settings")]
    public int initialEnemiesPerWave = 3;
    public int enemyIncreasePerWave = 2;

    public GameObject lanceObject;
    public GameObject xrRayInteractorObject;
    public destructibleGlobalMeshManager destructionManager;
    
    [Header("Effects")]
    public AlarmSystem alarmSystem;
    [Header("Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    public AudioClip buttonSound;

    private bool isGameActive = false;
    private bool isRestorationPhase = false;
    
    private int currentWave = 1;
    private int enemiesToSpawnCurrentWave;
    private int enemiesSpawnedCurrentWave;
    private float restorationTimer = 0f;

    void Start()
    {
        if (destructionManager != null)
        {
            destructionManager.OnEnemySpawned += OnEnemySpawnedHandler;
        }

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
        if (!isGameActive) return;

        // 1. Check Lose Condition (Strike Points)
        CheckLoseCondition();

        // 2. Handle Game Loop (Waves vs Restoration)
        if (isRestorationPhase)
        {
            restorationTimer -= Time.deltaTime;
            if (restorationTimer <= 0)
            {
                StartNextWave();
            }
        }
        else
        {
            // Wave is Active
            CheckWaveCompletion();
        }
    }

    private void CheckLoseCondition()
    {
        // Broker pieces count as '2', Enemy counts as '1'
        int enemyCount = FindObjectsByType<enemy>(FindObjectsSortMode.None).Length;
        int brokenWalls = 0;
        
        if (destructionManager != null) 
            brokenWalls = destructionManager.GetBrokenWallCount();

        int strikePoints = brokenWalls + enemyCount;

        // Optional: Update UI with strike points here if needed?

        if (strikePoints >= strikePointLimit)
        {
            Debug.Log($"Game Over! Strike Points: {strikePoints} (Limit: {strikePointLimit})");
            GameOver();
        }
    }

    private void CheckWaveCompletion()
    {
        // Check if we have spawned all enemies for this wave
        if (enemiesSpawnedCurrentWave >= enemiesToSpawnCurrentWave)
        {
            // Stop spawning
            if (destructionManager != null) destructionManager.SetDestructionActive(false);

            // Check if all enemies are dead
            int enemyCount = FindObjectsByType<enemy>(FindObjectsSortMode.None).Length;
            if (enemyCount == 0)
            {
                StartRestorationPhase();
            }
        }
    }

    private void OnEnemySpawnedHandler()
    {
        if (isGameActive && !isRestorationPhase)
        {
            enemiesSpawnedCurrentWave++;
        }
    }

    public void StartGame()
    {
        PlayButtonSound();

        // Fade out menu music, then start game music
        StartCoroutine(SwitchMusic(gameplayMusic, 1.0f));

        startCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
        
        isGameActive = true; 
        currentWave = 0; 
        
        if(lanceObject) lanceObject.SetActive(true);
        if(xrRayInteractorObject) xrRayInteractorObject.SetActive(false);

        StartCoroutine(StartGameSequence());
    }

    private System.Collections.IEnumerator StartGameSequence()
    {
        if (alarmSystem)
        {
             alarmSystem.TriggerAlarm("ENEMY BREACH", 3);
             float waitTime = (2.0f / alarmSystem.flickerSpeed) * 3.0f; 
             yield return new WaitForSeconds(waitTime + 0.5f);
        }

        StartNextWave();
    }

    private void StartNextWave()
    {
        currentWave++;
        isRestorationPhase = false;
        enemiesSpawnedCurrentWave = 0;
        
        // Wave 1: 5 segments, 1 enemy/seg
        // Wave 2: 5 segments, 2 enemies/seg
        // Wave 3: 5 segments, 3 enemies/seg
        // Wave 4+: Segments = 5 + (Wave-3)*2. Enemies/seg = 3.

        int segmentsToSpawn = 5;
        int enemiesPerSeg = 1;

        if (currentWave <= 3)
        {
            segmentsToSpawn = 5;
            enemiesPerSeg = currentWave;
        }
        else
        {
            segmentsToSpawn = 5 + ((currentWave - 3) * 2); 
            enemiesPerSeg = 3;
        }
        {
            destructionManager.enemiesPerBreach = enemiesPerSeg;
            float newInterval = Mathf.Max(2.5f, 7.0f - ((currentWave - 1) * 0.4f)); 
            destructionManager.destructionInterval = newInterval;
            
            destructionManager.SetDestructionActive(true);
        }

        enemiesToSpawnCurrentWave = segmentsToSpawn * enemiesPerSeg;

        if(alarmSystem) 
        {
            string msg = string.Format("WAVE {0}", currentWave);
            alarmSystem.TriggerAlarm(msg, 3);
        }
    }

    private void StartRestorationPhase()
    {
        isRestorationPhase = true;
        restorationTimer = restorationDuration;
        if(alarmSystem) alarmSystem.TriggerAlarm("RESTORE DEFENSES");
        
        Debug.Log("Restoration Phase Started");
    }

    public void GameOver()
    {
        isGameActive = false;
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

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }
        
        musicSource.volume = 0;
        musicSource.Stop();

        if (newClip != null)
        {
            musicSource.clip = newClip;
            musicSource.Play();
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