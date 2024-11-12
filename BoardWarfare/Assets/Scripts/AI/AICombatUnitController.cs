using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AICombatUnitController : MonoBehaviour
{
    GridSpawner gridSpawner;
    AIUnit aIUnit;
    EconomyManager economyManager;
    List<GameObject> enemiesToSpawn;
    public GameObject GroundUnit;
    public GameObject HeavyUnit;
    public GameObject AirUnit;
    private int budget;
    private bool hasSpawnedEnemies = false;

    // Start is called before the first frame update
    void Start()
    {
        gridSpawner = FindObjectOfType<GridSpawner>();
        enemiesToSpawn = new List<GameObject>();
        budget = 100;

        // Populate enemiesToSpawn based on the budget
        switch (budget)
        {
            case 100:
                for (int i = 0; i < 4; i++)
                {
                    enemiesToSpawn.Add(GroundUnit);
                    Debug.Log(enemiesToSpawn[i].name);
                }
                break;
        }

        Debug.Log("Enemies to spawn:");
        StartCoroutine(DelayedSpawnEnemies()); // Start coroutine for delayed spawning
    }

    // Coroutine to delay enemy spawning by 2 seconds
    private IEnumerator DelayedSpawnEnemies()
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds
        SpawnEnemies();
    }

    // Spawning enemies at random unoccupied positions in the bottom two rows
    public void SpawnEnemies()
    {
        if (hasSpawnedEnemies) return;  // Prevent multiple spawns
        hasSpawnedEnemies = true;

        int spawnedCount = 0;
        List<Vector2Int> availablePositions = new List<Vector2Int>();

        // Get the grid size from the GridSpawner
        int gridSize = gridSpawner.gridSize;

        // Collect all unoccupied tile positions in the two bottom rows
        foreach (var entry in gridSpawner.gridPositions)
        {
            Vector2Int position = entry.Key;
            if (!entry.Value.IsOccupied && (position.y == 0 || position.y == 1))
            {
                availablePositions.Add(position);
            }
        }

        // Randomly select tiles and spawn enemies
        while (spawnedCount < enemiesToSpawn.Count && availablePositions.Count > 0)
        {
            // Pick a random unoccupied tile from the available positions
            int randomIndex = UnityEngine.Random.Range(0, availablePositions.Count);
            Vector2Int tileCoord = availablePositions[randomIndex];

            Tile tile = gridSpawner.GetTileAtPosition(tileCoord);
            if (tile != null && !tile.IsOccupied)
            {
                // Mark the tile as occupied
                tile.IsOccupied = true;

                // Instantiate the enemy at the tile position
                GameObject enemy = enemiesToSpawn[spawnedCount];
                Instantiate(enemy, tile.Position, Quaternion.identity);

                spawnedCount++;
            }

            // Remove the chosen tile from the list of available positions
            availablePositions.RemoveAt(randomIndex);
        }

        if (spawnedCount < enemiesToSpawn.Count)
        {
            Debug.LogWarning("Not enough available tiles in the bottom two rows to spawn all enemies.");
        }
    }
}
