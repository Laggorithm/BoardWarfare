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
        // Shuffle the spots for randomness
        List<SpawnSpot> shuffledSpots = new List<SpawnSpot>(WallAndTallWallSpots);
        ShuffleList(shuffledSpots);

        // Determine the random number of walls and tall walls
        int totalSpawns = Mathf.Clamp(Random.Range(3, shuffledSpots.Count + 1), 3, shuffledSpots.Count);
        int wallCount = Random.Range(0, totalSpawns + 1);
        int tallWallCount = totalSpawns - wallCount;

        int spawned = 0;
        foreach (SpawnSpot spot in shuffledSpots)
        {
            if (spawned >= totalSpawns) break; // Stop if the total number is reached

            if (!spot.isOccupied)
            {
                GameObject prefabToSpawn = spawned < wallCount ? WallPrefab : TallWallPrefab;
                Instantiate(prefabToSpawn, spot.spotTransform.position, spot.spotTransform.rotation);
                spot.isOccupied = true; // Mark spot as occupied
                spawned++;
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
                Instantiate(prefab, spot.spotTransform.position, spot.spotTransform.rotation);
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
