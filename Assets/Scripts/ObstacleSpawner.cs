using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] obstaclePrefabs;
    public float spawnInterval = 1.2f;
    public float spawnZ = 40f; 
    public float laneDistance = 3f;

    private float timer;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnObstacle();
            timer = 0;
        }
    }

    void SpawnObstacle()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        // Pick a random lane and random prefab
        int lane = Random.Range(0, 3);
        int prefabIndex = Random.Range(0, obstaclePrefabs.Length);
        
        // Calculate spawn position
        Vector3 spawnPos = new Vector3((lane - 1) * laneDistance, 0, spawnZ);
        
        // Instantiate
        GameObject obstacle = Instantiate(obstaclePrefabs[prefabIndex], spawnPos, Quaternion.identity);
        
        // Ensure its moving and tagged correctly
        if (obstacle.GetComponent<WorldMover>() == null)
        {
            obstacle.AddComponent<WorldMover>();
        }
        obstacle.tag = "Obstacle";
    }
}
