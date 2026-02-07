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

    // Wave Progress Tracking
    private int segmentsToSpawnCurrentWave;
    private int segmentsSpawnedCurrentWave;
    
    private float restorationTimer = 0f;
    
    // SCORE SYSTEM
    public int score = 0;
    
    // Simple event if you want to update UI later
    // public System.ActionOnScoreChanged; 

    void Start()
    {
        if (destructionManager != null)
        {
            // destructionManager.OnEnemySpawned += OnEnemySpawnedHandler; // No longer needed for wave progress
            destructionManager.OnSegmentBroken += OnSegmentBrokenHandler;
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
        // 1. Have we spawned all wall breaks for this wave?
        if (segmentsSpawnedCurrentWave >= segmentsToSpawnCurrentWave)
        {
            // Stop spawning new breaks
            if (destructionManager != null) destructionManager.SetDestructionActive(false);

            // 2. Are all enemies dead?
            int enemyCount = FindObjectsByType<enemy>(FindObjectsSortMode.None).Length;
            if (enemyCount == 0)
            {
                // Wait a few seconds BEFORE starting restoration
                StartCoroutine(EndWaveRoutine());
            }
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log($"Score: {score}");
        // OnScoreChanged?.Invoke();
    }

    private System.Collections.IEnumerator EndWaveRoutine()
    {
        // Prevent double triggering
        if (isRestorationPhase) yield break;
        
        // Bonus Score for Wave Completion (100 * Wave Number)
        AddScore(100 * currentWave);
        
        // Wait a moment after killing last enemy (Scaling with wave?)
        yield return new WaitForSeconds(2.0f);

        StartRestorationPhase();
    }

    private void OnSegmentBrokenHandler()
    {
        if (isGameActive && !isRestorationPhase)
        {
            segmentsSpawnedCurrentWave++;
        }
    }
    
    // REMOVED: OnEnemySpawnedHandler (No longer used)

    public void StartGame()
    {
        PlayButtonSound();

        // Fade out menu music, then start game music
        StartCoroutine(SwitchMusic(gameplayMusic, 1.0f));

        startCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
        
        isGameActive = true; 
        currentWave = 0; 
        score = 0; // Reset Score
        
        if(lanceObject) lanceObject.SetActive(true);
        if(xrRayInteractorObject) xrRayInteractorObject.SetActive(false);

        StartCoroutine(StartGameSequence());
    }

    private System.Collections.IEnumerator StartGameSequence()
    {
        // 1. "ENEMY BREACH" (3 flickers)
        if (alarmSystem)
        {
             alarmSystem.TriggerAlarm("ENEMY BREACH", 3);
             float waitTime = (2.0f / alarmSystem.flickerSpeed) * 3.0f; 
             yield return new WaitForSeconds(waitTime + 0.5f);
        }

        // 2. "WAVE 1" (3 flickers)
        if (alarmSystem)
        {
             // We manually trigger this here so we can wait for it before starting the actual wave logic
             string msg = string.Format("WAVE {0}", 1);
             alarmSystem.TriggerAlarm(msg, 3);
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
            
            // Wave 1: 1 (min 1, max 1)
            // Wave 2: 1-2 (min 1, max 2)
            // Wave 3: 1-3 (min 1, max 3)
            // Rule: min is always 1, max is currentWave
            
            enemiesPerSeg = 0; // Not used for calculation anymore, handled by Average
        }
        else
        {
            // Starting Wave 4: 7 segments (+2 per wave after 3)
            segmentsToSpawn = 5 + ((currentWave - 3) * 2); 
            
            // Wave 4+: 1-3 (Wait, user said 1,2,3 or 4 for wave 10+. Let's scale max)
            // Let's make it 1 to 3 for Wave 4-9, and 1 to 4 for Wave 10+
            enemiesPerSeg = 0; 
        }

        // Apply rules to DestructionManager
        if (destructionManager != null)
        {
            // Reset Limits
            int minE = 1;
            int maxE = 1;

            if (currentWave == 1)      { minE = 1; maxE = 1; }
            else if (currentWave == 2) { minE = 1; maxE = 2; }
            else if (currentWave == 3) { minE = 1; maxE = 3; }
            else if (currentWave < 10) { minE = 1; maxE = 3; } // Waves 4-9
            else                       { minE = 1; maxE = 4; } // Waves 10+

            destructionManager.minEnemiesPerBreach = minE;
            destructionManager.maxEnemiesPerBreach = maxE;

            // NEW: We now track wave progress based on SEGMENTS spawned
            // The wave ends when all segments have broken + all enemies are dead
            segmentsToSpawnCurrentWave = segmentsToSpawn;
            segmentsSpawnedCurrentWave = 0;

            float newInterval = Mathf.Max(2.5f, 7.0f - ((currentWave - 1) * 0.4f)); 
            destructionManager.destructionInterval = newInterval;
            
            destructionManager.SetDestructionActive(true);
        }

            float newInterval = Mathf.Max(2.5f, 7.0f - ((currentWave - 1) * 0.4f)); 
            destructionManager.destructionInterval = newInterval;
            
            destructionManager.SetDestructionActive(true);
        }

        if(alarmSystem && currentWave > 1) 
        {
            string msg = string.Format("WAVE {0}", currentWave);
            alarmSystem.TriggerAlarm(msg, 3);
        }
    }

    private void StartRestorationPhase()
    {
        isRestorationPhase = true;
        restorationTimer = restorationDuration;
        
        // Wait a few seconds BEFORE showing the "RESTORE WALL" alarm
        // Logic: "give the player a few seconds (based on which wave they are on)"
        // Let's say: Wave 1 = 3s, Wave 10 = 1s? Or maybe longer for later waves to catch breath?
        // Usually "catch breath" implies longer wait. 
        // Let's do: 3 seconds base + 0.5s per wave (Max 8s)
        
        float preAlarmDelay = Mathf.Min(8.0f, 3.0f + (currentWave * 0.5f));
        StartCoroutine(RestorationAlarmRoutine(preAlarmDelay));

        Debug.Log("Restoration Phase Started");
    }

    private System.Collections.IEnumerator RestorationAlarmRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if(alarmSystem) alarmSystem.TriggerAlarm("RESTORE WALL");
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