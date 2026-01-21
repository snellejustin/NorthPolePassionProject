using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enemySpawner spawner;
    public GameObject startCanvas;
    public GameObject gameOverCanvas;
    public int maxEnemies = 15;

    public GameObject lanceObject;
    public GameObject xrRayInteractorObject;
    public destructibleGlobalMeshManager destructionManager;

    void Start()
    {
        ShowStartScreen();
    }

    void Update()
    {
        // Only check for game over if the game is running
        if (spawner.isSpawning)
        {
            // Count active enemies
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
        spawner.isSpawning = true;
        
        if(lanceObject) lanceObject.SetActive(true);
        if(xrRayInteractorObject) xrRayInteractorObject.SetActive(false);
        if(destructionManager) destructionManager.SetDestructionActive(true);
    }

    public void GameOver()
    {
        spawner.isSpawning = false;
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
        spawner.isSpawning = false;

        if(lanceObject) lanceObject.SetActive(false);
        if(xrRayInteractorObject) xrRayInteractorObject.SetActive(true);
        if(destructionManager) destructionManager.SetDestructionActive(false);
    }
}
