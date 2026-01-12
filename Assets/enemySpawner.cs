using UnityEngine;

public class enemySpawner : MonoBehaviour
{
    public float spawnTimer = 1;
    public GameObject prefabToSpawn;
    public float spawnRadius = 3;
    private float timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > spawnTimer)
        {
            SpawnEnemy();
            timer -= spawnTimer;
        }
    }

    public void SpawnEnemy()
    {
        Vector3 randomSpawnPosition = Random.insideUnitSphere * spawnRadius;
        randomSpawnPosition.y = 0;
        Instantiate(prefabToSpawn, randomSpawnPosition, Quaternion.identity);
    }
}
