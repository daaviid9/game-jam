using UnityEngine;
using System.Collections.Generic;

public class EnvironmentSpawner : MonoBehaviour
{
    [Header("Priradenie Prefabov")]
    [Tooltip("Zoznam prefabov pre úseky cesty. Ak ich bude viac, budú sa vyberať náhodne.")]
    public GameObject[] chunkPrefabs;

    [Header("Nastavenia Generovania")]
    [Tooltip("Dĺžka jedného úseku pozdĺž osi Z. Uisti sa, že toto číslo presne sedí s tvojím 3D modelom, inak vzniknú medzery.")]
    public float chunkLength = 20f;

    [Tooltip("Koľko úsekov má byť viditeľných naraz v jednej chvíli.")]
    public int numberOfChunksOnScreen = 6;

    [Tooltip("Na akej Z pozícii má začať generovanie cesty (napr. trošku za hráčom, aby nevidel koniec cesty za sebou)")]
    public float startZPosition = -20f;

    [Tooltip("Pozícia Z za hráčom, pri ktorej už objekt nevidíme a môžeme ho bezpečne zmazať.")]
    public float destroyZPosition = -30f;

    // Uchovávanie momentálne aktívnych úsekov
    private List<GameObject> activeChunks = new List<GameObject>();

    void Start()
    {
        // Úvodné vygenerovanie série úsekov cesty
        for (int i = 0; i < numberOfChunksOnScreen; i++)
        {
            SpawnChunk(startZPosition + (i * chunkLength));
        }
    }

    void Update()
    {
        // Ak hra skončila, nebudeme riešiť nové úseky
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        // Skontrolujeme najstarší (prvý) úsek cesty, či sa neposunul za hranicu destroyZPosition
        if (activeChunks.Count > 0 && activeChunks[0].transform.position.z < destroyZPosition)
        {
            // Vymažeme ten starý, lebo tam už hráč nevidí
            DeleteOldestChunk();
            
            // Zistíme presnú pozíciu Z úplne najnovšieho úseku, ktorý je najďalej vpredu
            float lastChunkZ = activeChunks[activeChunks.Count - 1].transform.position.z;
            
            // A tesne za neho vygenerujeme ďalší
            SpawnChunk(lastChunkZ + chunkLength);
        }
    }

    private void SpawnChunk(float spawnZPoint)
    {
        if (chunkPrefabs == null || chunkPrefabs.Length == 0) return;

        // Vyberie náhodný úsek zo zoznamu (ideálne, ak máš napr. rôzne rozmiestnené stromy)
        int prefabIndex = Random.Range(0, chunkPrefabs.Length);
        
        // Vytvorenie inštancie s nulovým posunom X, Y a s určeným Z posunom
        Vector3 spawnPosition = new Vector3(0, 0, spawnZPoint);
        GameObject newChunk = Instantiate(chunkPrefabs[prefabIndex], spawnPosition, Quaternion.identity);
        
        // Zabezpečme pre úsek aj automatický pohyb, ak sme ho zabudli dať na prefab
        if (newChunk.GetComponent<WorldMover>() == null)
        {
            newChunk.AddComponent<WorldMover>();
        }

        // Tagneme mu iný tag namiesto "Obstacle", aby si ho WorldMover neposlúchol a nezničil ho priskoro na Z=-15
        newChunk.tag = "Untagged"; // Alebo si pre to sprav tag "Environment"
        
        // Uloženie do pamäti (listu)
        activeChunks.Add(newChunk);
    }

    private void DeleteOldestChunk()
    {
        GameObject oldestChunk = activeChunks[0];
        activeChunks.RemoveAt(0);
        Destroy(oldestChunk);
    }
}
