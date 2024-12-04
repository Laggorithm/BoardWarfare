using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnSpot
    {
        public Transform spotTransform; // Position and rotation of the spawn spot
        public bool isOccupied = false; // Tracks if the spot is already used
    }

    // Unified spawn spots for walls and tall walls
    public List<SpawnSpot> WallAndTallWallSpots = new List<SpawnSpot>();
    public List<SpawnSpot> HealSpots = new List<SpawnSpot>();

    public GameObject WallPrefab;
    public GameObject TallWallPrefab;
    public GameObject HealPrefab;

    private void Start()
    {
        Debug.Log("SpawnManager Start called.");

        if (WallAndTallWallSpots.Count == 0)
        {
            Debug.LogError("WallAndTallWallSpots is empty. Assign spawn spots in the Inspector.");
        }

        if (WallPrefab == null || TallWallPrefab == null)
        {
            Debug.LogError("WallPrefab or TallWallPrefab is not assigned in the Inspector.");
        }

        SpawnRandomizedWallPrefabs();
    }

    public void SpawnPrefabs()
    {
        // Check if minimum requirements for wall spots are met
        if (WallAndTallWallSpots.Count < 3)
        {
            Debug.LogError("Minimum spawn spot requirements for walls and tall walls are not met.");
            return;
        }

        // Spawn walls and tall walls (shared spots)
        SpawnRandomizedWallPrefabs();

        // Spawn heals
        SpawnRandomizedPrefabs(HealSpots, HealPrefab, 0, 3);
    }

    private void SpawnRandomizedWallPrefabs()
    {
        List<SpawnSpot> shuffledSpots = new List<SpawnSpot>(WallAndTallWallSpots);
        ShuffleList(shuffledSpots);

        int totalSpawns = Random.Range(3, shuffledSpots.Count + 1);
        int wallCount = Random.Range(0, totalSpawns + 1);
        int tallWallCount = totalSpawns - wallCount;

        Debug.Log($"Total spots: {shuffledSpots.Count}, Walls: {wallCount}, Tall Walls: {tallWallCount}");

        int spawned = 0;
        foreach (SpawnSpot spot in shuffledSpots)
        {
            if (spawned >= totalSpawns) break;

            if (!spot.isOccupied)
            {
                GameObject prefabToSpawn = spawned < wallCount ? WallPrefab : TallWallPrefab;
                Debug.Log($"Attempting to spawn {prefabToSpawn.name} at {spot.spotTransform.position}");

                // Instantiate the prefab and mark the spot as occupied, adjusting Y to 0.6f for walls
                Vector3 spawnPosition = new Vector3(spot.spotTransform.position.x, 0.6f, spot.spotTransform.position.z);
                GameObject spawnedPrefab = Instantiate(prefabToSpawn, spawnPosition, spot.spotTransform.rotation);

                // If the prefab is a Wall, rotate it by -90 degrees on the Y-axis
                if (prefabToSpawn == WallPrefab)
                {
                    spawnedPrefab.transform.Rotate(0f, -90f, 0f);
                }

                spot.isOccupied = true;

                Debug.Log($"Spot at {spot.spotTransform.position} marked as occupied.");
                spawned++;
            }
            else
            {
                Debug.Log($"Spot at {spot.spotTransform.position} is already occupied.");
            }
        }
    }

    private void SpawnRandomizedPrefabs(List<SpawnSpot> spots, GameObject prefab, int minCount, int maxCount)
    {
        // Shuffle the spots to ensure randomness
        List<SpawnSpot> shuffledSpots = new List<SpawnSpot>(spots);
        ShuffleList(shuffledSpots);

        // Determine the number of prefabs to spawn
        int prefabCount = Mathf.Clamp(Random.Range(minCount, maxCount + 1), minCount, shuffledSpots.Count);

        int spawned = 0;
        foreach (SpawnSpot spot in shuffledSpots)
        {
            if (spawned >= prefabCount) break; // Stop if the desired number is reached

            if (!spot.isOccupied)
            {
                // Adjust the Y position to 5 before spawning
                Vector3 spawnPosition = new Vector3(spot.spotTransform.position.x, 5f, spot.spotTransform.position.z);
                Instantiate(prefab, spawnPosition, spot.spotTransform.rotation);
                spot.isOccupied = true; // Mark spot as occupied
                spawned++;
            }
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
