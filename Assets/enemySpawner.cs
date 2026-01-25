using UnityEngine;
using Meta.XR.MRUtilityKit;

public class enemySpawner : MonoBehaviour
{
    public float spawnTimer = 5; // Increased from 2 for testing
    public GameObject prefabToSpawn;
    public float spawnRadius = 3;
    private float timer;

    public float minEdgeDistance = 0.3f;
    public MRUKAnchor.SceneLabels spawnLabels;
    public float normalOffset;

    public int spawnTry = 1000;
    public bool isSpawning = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSpawning) return;

        if (MRUK.Instance == null || !MRUK.Instance.IsInitialized)
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer > spawnTimer)
        {
            SpawnEnemy();
            timer -= spawnTimer;
        }
    }

    public void SpawnEnemy()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null) return;

        int currentTry = 0;
        while (currentTry < spawnTry)
        {
            bool hasFoundPosition = room.GenerateRandomPositionOnSurface(MRUK.SurfaceType.VERTICAL, minEdgeDistance, new LabelFilter(spawnLabels), out Vector3 pos, out Vector3 norm);
            if(hasFoundPosition)
            {
                Vector3 randomSpawnPositionNormalOffset = pos + norm * normalOffset;
            randomSpawnPositionNormalOffset.y = 0;
            Instantiate(prefabToSpawn, randomSpawnPositionNormalOffset, Quaternion.identity);

            return;
        }
        currentTry++;
    }
}
}