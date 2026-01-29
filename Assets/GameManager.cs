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

    // Nieuwe variabele om bij te houden of het spel bezig is
    private bool isGameActive = false;

    void Start()
    {
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
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