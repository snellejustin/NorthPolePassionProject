using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enemySpawner spawner;
    public GameObject startCanvas;
    public GameObject gameOverCanvas;
    public int maxEnemies = 15;

    public GameObject lanceObject;
    public GameObject mrPointerObject;

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
            int enemyCount = FindObjectsOfType<enemy>().Length;
            
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
        if(mrPointerObject) mrPointerObject.SetActive(false);
    }

    public void GameOver()
    {
        spawner.isSpawning = false;
        gameOverCanvas.SetActive(true);

        if(lanceObject) lanceObject.SetActive(false);
        if(mrPointerObject) mrPointerObject.SetActive(true);
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
        if(mrPointerObject) mrPointerObject.SetActive(true);
    }
}
