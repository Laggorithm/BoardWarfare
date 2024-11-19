using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallSpawner : MonoBehaviour
{
    GridSpawner gridSpawner;
    public GameObject TallWall;
    public GameObject Smolwall;

    void Start()
    {
        // Find the GridSpawner instance and wait to spawn walls
        gridSpawner = FindObjectOfType<GridSpawner>();
        StartCoroutine(DelayedSpawnWalls());
    }

    private IEnumerator DelayedSpawnWalls()
    {
        // Wait to ensure the grid is generated before spawning walls
        yield return new WaitForSeconds(2.5f);
        SpawnWalls();
    }

    // Spawning walls randomly across the grid
    private void SpawnWalls()
    {
        List<Vector2Int> availablePositions = new List<Vector2Int>();

        // Get grid dimensions from GridSpawner
        int rows = gridSpawner.rows;
        int columns = gridSpawner.columns;

        // Collect all unoccupied positions in the grid
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GameObject tileObject = gridSpawner.gridArray[x, y];
                if (tileObject != null && !tileObject.GetComponent<GridStat>().IsOccupied)
                {
                    availablePositions.Add(new Vector2Int(x, y));
                }
            }
        }

        // Define how many tall and small walls to spawn
        int tallWallCount = GetRandomNumber(5, 7);
        int smolWallCount = GetRandomNumber(5, 7);

        // Spawn specific walls (tall and small)
        SpawnSpecificWall(TallWall, tallWallCount, availablePositions);
        SpawnSpecificWall(Smolwall, smolWallCount, availablePositions);
    }

    private void SpawnSpecificWall(GameObject wallPrefab, int count, List<Vector2Int> availablePositions)
    {
        int spawnedCount = 0;

        while (spawnedCount < count && availablePositions.Count > 0)
        {
            int randomIndex = Random.Range(0, availablePositions.Count);
            Vector2Int chosenPosition = availablePositions[randomIndex];
            GameObject tileObject = gridSpawner.gridArray[chosenPosition.x, chosenPosition.y];

            // Ensure the tile is unoccupied before spawning the wall
            if (tileObject != null)
            {
                tileObject.GetComponent<GridStat>().IsOccupied = true;

                // Set the spawn position, ensuring the wall is always at Y=10
                Vector3 spawnPosition = tileObject.transform.position;
                spawnPosition.y = 10f; // Set height to 10

                // Randomize rotation between 0 and 90 degrees (for variety in wall orientation)
                float randomYRotation = Random.Range(0, 2) * 90; // 0 or 90 degrees
                Quaternion rotation = Quaternion.Euler(0, randomYRotation, 0);

                // Instantiate the wall at the chosen position and rotation
                Instantiate(wallPrefab, spawnPosition, rotation);

                spawnedCount++;
            }

            // Remove the chosen position from available positions to prevent overlap
            availablePositions.RemoveAt(randomIndex);
        }

        // Log a warning if not enough positions are available to spawn all walls
        if (spawnedCount < count)
        {
            Debug.LogWarning($"Not enough available tiles to spawn all {wallPrefab.name} walls.");
        }
    }

    // Helper function to generate a random number between min and max (inclusive)
    private int GetRandomNumber(int min, int max)
    {
        return Random.Range(min, max + 1); // Include max value
    }
}
