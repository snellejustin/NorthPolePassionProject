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

    private int segmentsToSpawnCurrentWave;
    private int segmentsSpawnedCurrentWave;
    
    private float restorationTimer = 0f;
    
    public int score = 0;
    public TMPro.TMP_Text scoreText; 
    public TMPro.TMP_Text gameOverScoreText; 


    void Start()
    {
        if (destructionManager != null)
        {
            destructionManager.OnSegmentBroken += OnSegmentBrokenHandler;
        }

        if (musicSource && menuMusic)
        {
            musicSource.clip = menuMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        if (scoreText == null)
        {
 
            GameObject stObj = GameObject.Find("Score Counter"); 
            if (stObj != null)
            {
                 var tmp = stObj.GetComponentInChildren<TMPro.TMP_Text>();
                 if (tmp != null) scoreText = tmp;
            }
        }

        ShowStartScreen();
    }

    void Update()
    {
        if (!isGameActive) return;

        CheckLoseCondition();

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
            CheckWaveCompletion();
        }
    }

    private bool isWaveEnding = false;

    public int CurrentStrikePoints { get; private set; }

    private void CheckLoseCondition()
    {
        int enemyCount = FindObjectsByType<enemy>(FindObjectsSortMode.None).Length;
        int brokenWalls = 0;
        
        if (destructionManager != null) 
            brokenWalls = destructionManager.GetBrokenWallCount();

        int strikePoints = brokenWalls + enemyCount;
        CurrentStrikePoints = strikePoints; 


        if (strikePoints >= strikePointLimit)
        {
            Debug.Log($"Game Over! Strike Points: {strikePoints} (Limit: {strikePointLimit})");
            GameOver();
        }
    }

    private void CheckWaveCompletion()
    {
        if (isWaveEnding || isRestorationPhase) return;

        if (segmentsSpawnedCurrentWave >= segmentsToSpawnCurrentWave)
        {
            if (destructionManager != null) destructionManager.SetDestructionActive(false);

            int enemyCount = FindObjectsByType<enemy>(FindObjectsSortMode.None).Length;
            if (enemyCount == 0)
            {
                StartCoroutine(EndWaveRoutine());
            }
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (scoreText != null) scoreText.text = score.ToString();
        Debug.Log($"Score: {score}");
    }

    private System.Collections.IEnumerator EndWaveRoutine()
    {
        if (isWaveEnding || isRestorationPhase) yield break;
        
        isWaveEnding = true; 
        AddScore(100 * currentWave);
        
        yield return new WaitForSeconds(2.0f);

        StartRestorationPhase();
        isWaveEnding = false; 
    }

    private void OnSegmentBrokenHandler()
    {
        if (isGameActive && !isRestorationPhase && !isWaveEnding)
        {
            segmentsSpawnedCurrentWave++;
        }
    }
    
    public void StartGame()
    {
        PlayButtonSound();

        StartCoroutine(SwitchMusic(gameplayMusic, 1.0f));

        startCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
        
        isGameActive = true; 
        currentWave = 0; 
        score = 0; 
        if (scoreText != null) scoreText.text = "0";
        
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
        if (alarmSystem)
        {
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

        int segmentsToSpawn = 5;


        if (currentWave <= 3)
        {
            segmentsToSpawn = 5;
        
        }
        else
        {
            segmentsToSpawn = 5 + ((currentWave - 3) * 2); 
        }
        if (destructionManager != null)
        {
            int minE = 1;
            int maxE = 1;

            if (currentWave == 1)      { minE = 1; maxE = 1; }
            else if (currentWave == 2) { minE = 1; maxE = 2; }
            else if (currentWave == 3) { minE = 1; maxE = 3; }
            else if (currentWave < 10) { minE = 1; maxE = 3; } 
            else                       { minE = 1; maxE = 4; } 

            destructionManager.minEnemiesPerBreach = minE;
            destructionManager.maxEnemiesPerBreach = maxE;

            if (currentWave < 3) 
            {
                destructionManager.spawnBehindWallDistance = 4.0f;
            }
            else if (currentWave < 5)
            {
                destructionManager.spawnBehindWallDistance = 2.0f;
            }
            else
            {
                destructionManager.spawnBehindWallDistance = 1.0f;
            }

            segmentsToSpawnCurrentWave = segmentsToSpawn;
            segmentsSpawnedCurrentWave = 0;

            float newInterval = Mathf.Max(2.5f, 7.0f - ((currentWave - 1) * 0.4f)); 
            destructionManager.destructionInterval = newInterval;
            
            destructionManager.SetDestructionActive(true);
        }



        if(alarmSystem && currentWave >= 1) 
        {
            string msg = string.Format("WAVE {0}", currentWave);
            if(currentWave > 1) alarmSystem.TriggerAlarm(msg, 3);
        }
    }

    private void StartRestorationPhase()
    {
        isRestorationPhase = true;
        restorationTimer = restorationDuration;
        
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

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "FINAL SCORE: " + score.ToString();
        }

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