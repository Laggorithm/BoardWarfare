using System.Collections;
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

    [System.Serializable]
    public class Wave
    {
        public int ID; // Wave ID
        public int GroundMobs; // Number of ground mobs
        public int HeavyMobs; // Number of heavy mobs
        public int AirMobs; // Number of air mobs
        public static float WaveHardnessLevel;
    }

    // Unified spawn spots for walls and tall walls
    public List<SpawnSpot> WallAndTallWallSpots = new List<SpawnSpot>();
    public List<SpawnSpot> HealSpots = new List<SpawnSpot>();

    // Separate spots for mobs
    public List<SpawnSpot> MobSpots = new List<SpawnSpot>();
    public List<Wave> Waves = new List<Wave>(); // Preset waves for mobs

    public GameObject WallPrefab;
    public GameObject TallWallPrefab;
    public GameObject HealPrefab;

    public GameObject GroundMobPrefab;
    public GameObject HeavyMobPrefab;
    public GameObject AirMobPrefab;

    private List<GameObject> spawnedWalls = new List<GameObject>();
    private List<GameObject> spawnedTallWalls = new List<GameObject>();
    private List<GameObject> spawnedMobs = new List<GameObject>();

    public int waveEffectValue = 0;
    public string WaveEffectName;
    private void Start()
    {
        

        Debug.Log("SpawnManager Start called.");

        if (WallAndTallWallSpots.Count == 0)
            Debug.LogError("WallAndTallWallSpots is empty. Assign spawn spots in the Inspector.");

        if (MobSpots.Count == 0)
            Debug.LogError("MobSpots is empty. Assign spawn spots in the Inspector.");

        if (WallPrefab == null || TallWallPrefab == null || GroundMobPrefab == null || HeavyMobPrefab == null || AirMobPrefab == null)
            Debug.LogError("One or more prefabs are not assigned in the Inspector.");

        SpawnRandomizedWallPrefabs();

        // Randomly pick and execute a wave
        ExecuteRandomWave();
        RollWaveEffect();
        StartCoroutine(CheckMapForMobs());
        
    }
    private IEnumerator CheckMapForMobs()
    {
        while (true)
        {
            // Remove null (destroyed) entries from the list
            spawnedMobs.RemoveAll(mob => mob == null);

            // If all tracked mobs are destroyed, clear the map and start a new wave
            if (spawnedMobs.Count == 0)
            {
                Debug.Log("All tracked mobs are destroyed. Clearing the map and spawning a new wave.");

                ClearSpawnedObjects(); // Clear the map
                yield return new WaitForSeconds(1f); // Small delay for clearing

                SpawnRandomizedWallPrefabs(); // Spawn new walls and tall walls
                ExecuteRandomWave(); // Spawn a new random wave
                RollWaveEffect();
                 
                
            }

            // Wait before the next check
            yield return new WaitForSeconds(1f);
        }
    }

    public int RollWaveEffect()
    {
        int rolledNumber = Random.Range(1, 5);  // Generates a number between 1 and 4
        waveEffectValue = rolledNumber;  // Set the wave effect value to the rolled number

        Debug.Log($"Wave Effect Rolled: {rolledNumber}. Current total: {waveEffectValue}");
        

        return rolledNumber;  // Return the rolled number
    }

    private void ClearSpawnedObjects()
    {
        // Destroy all mobs
        foreach (GameObject mob in spawnedMobs)
        {
            if (mob != null) Destroy(mob);
        }
        spawnedMobs.Clear(); // Clear the tracking list for mobs

        // Destroy all walls
        foreach (GameObject wall in spawnedWalls)
        {
            if (wall != null) Destroy(wall);
        }
        spawnedWalls.Clear(); // Clear the tracking list for walls

        // Destroy all tall walls
        foreach (GameObject tallWall in spawnedTallWalls)
        {
            if (tallWall != null) Destroy(tallWall);
        }
        spawnedTallWalls.Clear(); // Clear the tracking list for tall walls

        // Reset the "isOccupied" flag for all spawn spots
        foreach (SpawnSpot spot in WallAndTallWallSpots)
            spot.isOccupied = false;

        foreach (SpawnSpot spot in HealSpots)
            spot.isOccupied = false;

        foreach (SpawnSpot spot in MobSpots)
            spot.isOccupied = false;

        Debug.Log("Cleared all spawned objects and reset spawn spots.");
    }



    private void DestroyAllWithTag(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objects)
        {
            Destroy(obj);
        }
    }

    public void SpawnPrefabs()
    {
        // Spawn walls and tall walls (shared spots)
        SpawnRandomizedWallPrefabs();

        // Spawn heals
        SpawnRandomizedPrefabs(HealSpots, HealPrefab, 0, 3);
    }

    public void ExecuteWave(int waveID)
    {
        Wave.WaveHardnessLevel = Random.Range(1, 3);
        Debug.Log(Wave.WaveHardnessLevel);
        // Find the wave with the given ID
        Wave wave = Waves.Find(w => w.ID == waveID);
        if (wave == null)
        {
            Debug.LogError($"Wave with ID {waveID} not found!");
            return;
        }

        Debug.Log($"Executing Wave {wave.ID}: Ground {wave.GroundMobs}, Heavy {wave.HeavyMobs}, Air {wave.AirMobs}");

        // Spawn mobs for the wave
        SpawnMobs(GroundMobPrefab, wave.GroundMobs);
        SpawnMobs(HeavyMobPrefab, wave.HeavyMobs);
        SpawnMobs(AirMobPrefab, wave.AirMobs);
    }
    public void ExecuteRandomWave()
    {
        // Check if there are any waves in the list
        if (Waves.Count == 0)
        {
            Debug.LogError("No waves available to execute.");
            return;
        }

        // Ensure the waves are sorted by ID (if not already sorted)
        Waves.Sort((wave1, wave2) => wave1.ID.CompareTo(wave2.ID));

        // Get the minimum and maximum wave ID
        int minWaveID = Waves[0].ID;
        int maxWaveID = Waves[Waves.Count - 1].ID;

        // Pick a random wave ID between min and max wave ID
        int randomWaveID = Random.Range(minWaveID, maxWaveID + 1);

        // Find the wave with the selected random wave ID
        Wave selectedWave = Waves.Find(w => w.ID == randomWaveID);

        if (selectedWave == null)
        {
            Debug.LogError($"No wave found with ID {randomWaveID}. Something went wrong.");
            return;
        }

        // Log and execute the selected wave
        Debug.Log($"Executing Random Wave {selectedWave.ID}: Ground {selectedWave.GroundMobs}, Heavy {selectedWave.HeavyMobs}, Air {selectedWave.AirMobs}");

        ExecuteWave(selectedWave.ID);
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

                // Instantiate the prefab and adjust Y to 0.6f for walls
                Vector3 spawnPosition = new Vector3(spot.spotTransform.position.x, 0.6f, spot.spotTransform.position.z);
                GameObject spawnedPrefab = Instantiate(prefabToSpawn, spawnPosition, spot.spotTransform.rotation);

                // If the prefab is a Wall, rotate it by -90 degrees on the Y-axis
                if (prefabToSpawn == WallPrefab)
                {
                    spawnedPrefab.transform.Rotate(0f, -90f, 0f);
                    spawnedWalls.Add(spawnedPrefab); // Track spawned wall
                }
                else
                {
                    spawnedTallWalls.Add(spawnedPrefab); // Track spawned tall wall
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

    public void SpawnMobs(GameObject prefab, int count)
    {
        if (count <= 0 || prefab == null) return;

        // Shuffle spawn spots for randomness
        List<SpawnSpot> shuffledSpots = new List<SpawnSpot>(MobSpots);
        ShuffleList(shuffledSpots);

        int spawned = 0;
        foreach (SpawnSpot spot in shuffledSpots)
        {
            if (spawned >= count) break;

            if (!spot.isOccupied)
            {
                // Adjust the Y position to 5 for spawning mobs
                Vector3 spawnPosition = new Vector3(spot.spotTransform.position.x, 5f, spot.spotTransform.position.z);
                GameObject mob = Instantiate(prefab, spawnPosition, spot.spotTransform.rotation);

                // Add the mob to the list for tracking
                spawnedMobs.Add(mob);

                spot.isOccupied = true; // Mark spot as occupied
                spawned++;
            }
        }

        Debug.Log($"Spawned {spawned}/{count} {prefab.name}(s).");
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
