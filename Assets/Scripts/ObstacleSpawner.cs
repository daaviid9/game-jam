using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Prekážky - Kategórie")]
    public GameObject[] carPrefabs;
    public GameObject[] edgeObstacles;    // napr. smetiaky, stĺpy
    public GameObject[] barricadePrefabs; // napr. ploty, barely

    [Header("Šance spawnu (Súčet ideálne 100%)")]
    [Range(0, 100)] public float carChance = 60f;
    [Range(0, 100)] public float edgeChance = 20f;
    [Range(0, 100)] public float barricadeChance = 20f;

    [Header("Spawn Nastavenia")]
    [Tooltip("Vzdialenosť v metroch medzi vygenerovanými prekážkami. Čím menšie číslo, tým hustejšia premávka.")]
    public float spawnDistance = 14.5f;
    public float spawnZ = 40f; 
    public float laneDistance = 3f;
    
    [Tooltip("Výška (Y), v ktorej sa prekážky objavia. Zmeň do mínusu, ak lietajú nad zemou.")]
    public float spawnYOffset = 0f;

    private float distanceTraveled;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        // Pripočítavame vzdialenosť podľa aktuálnej rýchlosti sveta
        distanceTraveled += WorldMover.moveSpeed * Time.deltaTime;
        
        // Ak sme prešli požadovanú vzdialenosť, vygeneruje sa prekážka
        if (distanceTraveled >= spawnDistance)
        {
            SpawnSophisticatedObstacle();
            distanceTraveled = 0; // Reset vzdialenosti do ďalšej prekážky
        }
    }

    void SpawnSophisticatedObstacle()
    {
        float roll = Random.Range(0f, 100f);

        if (roll < carChance)
        {
            // --- SPAWN AUTO (100 DMG) ---
            if (carPrefabs.Length == 0) return;
            int lane = Random.Range(0, 3); 
            SpawnSingle(carPrefabs, lane, 100);
        }
        else if (roll < carChance + edgeChance)
        {
            // --- SPAWN OKRAJOVÝ OBJEKT (34 DMG) ---
            if (edgeObstacles.Length == 0) return;
            int lane = Random.value > 0.5f ? 0 : 2;
            SpawnSingle(edgeObstacles, lane, 34);
        }
        else
        {
            // --- SPAWN ZÁTARASY (50 DMG) ---
            if (barricadePrefabs.Length == 0) return;
            
            bool twoBarricades = Random.value < 0.4f;

            if (twoBarricades)
            {
                int safeLane = Random.Range(0, 3);
                for (int i = 0; i < 3; i++)
                {
                    if (i != safeLane)
                    {
                        SpawnSingle(barricadePrefabs, i, 50);
                    }
                }
            }
            else
            {
                int lane = Random.Range(0, 3);
                SpawnSingle(barricadePrefabs, lane, 50);
            }
        }
    }

    void SpawnSingle(GameObject[] array, int lane, int damageAmount)
    {
        int prefabIndex = Random.Range(0, array.Length);
        GameObject prefabToSpawn = array[prefabIndex];

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("[ObstacleSpawner] Pozor! V poli prekážok ti chýba model (je tam 'None'). Preskakujem spawn.");
            return;
        }

        // Vypočítať pozíciu (lane 0 je -3, lane 1 je 0, lane 2 je 3)
        Vector3 spawnPos = new Vector3((lane - 1) * laneDistance, spawnYOffset, spawnZ);
        
        GameObject obstacle = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        
        if (obstacle.GetComponent<WorldMover>() == null)
        {
            obstacle.AddComponent<WorldMover>();
        }
        
        // Dynamicky mu vlepíme náš nový skript s dátami o zranení
        if (obstacle.GetComponent<ObstacleData>() == null)
        {
            ObstacleData data = obstacle.AddComponent<ObstacleData>();
            data.damageAmount = damageAmount;
        }

        // Nastavíme tag, aby fungovali kolízie s hráčom
        obstacle.tag = "Obstacle";
    }
}
