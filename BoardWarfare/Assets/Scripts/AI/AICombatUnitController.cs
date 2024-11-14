using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICombatUnitController : MonoBehaviour
{
    GridSpawner gridSpawner;
    List<GameObject> enemiesToSpawn;
    public GameObject GroundUnit;
    public GameObject HeavyUnit;
    public GameObject AirUnit;
    private int budget;
    private bool hasSpawnedEnemies = false;

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
                }
                break;
        }

        StartCoroutine(DelayedSpawnEnemies());
    }

    private IEnumerator DelayedSpawnEnemies()
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds to ensure the grid is generated
        SpawnEnemies();
    }

    // Spawning enemies at random unoccupied positions in the bottom two rows
    private void SpawnEnemies()
    {
        if (hasSpawnedEnemies) return;  // Prevent multiple spawns
        hasSpawnedEnemies = true;

        int spawnedCount = 0;
        List<Vector2Int> availablePositions = new List<Vector2Int>();

        // Get the grid dimensions from GridSpawner
        int rows = gridSpawner.rows;
        int columns = gridSpawner.columns;

        // Collect all unoccupied positions in the bottom two rows
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < 2; y++) // Only the bottom two rows
            {
                GameObject tileObject = gridSpawner.gridArray[x, y];
                if (tileObject != null && !tileObject.GetComponent<GridStat>().IsOccupied)
                {
                    availablePositions.Add(new Vector2Int(x, y));
                }
            }
        }

        // Randomly select tiles and spawn enemies
        while (spawnedCount < enemiesToSpawn.Count && availablePositions.Count > 0)
        {
            int randomIndex = Random.Range(0, availablePositions.Count);
            Vector2Int chosenPosition = availablePositions[randomIndex];
            GameObject tileObject = gridSpawner.gridArray[chosenPosition.x, chosenPosition.y];

            // Mark the tile as occupied and spawn the enemy
            if (tileObject != null)
            {
                tileObject.GetComponent<GridStat>().IsOccupied = true;

                GameObject enemy = enemiesToSpawn[spawnedCount];
                // Spawn the enemy at the tile's position with Y set to 5
                Vector3 spawnPosition = tileObject.transform.position;
                spawnPosition.y = 5f;

                Instantiate(enemy, spawnPosition, Quaternion.identity);

                spawnedCount++;
            }

            availablePositions.RemoveAt(randomIndex);
        }

        if (spawnedCount < enemiesToSpawn.Count)
        {
            Debug.LogWarning("Not enough available tiles in the bottom two rows to spawn all enemies.");
        }
    }
}
