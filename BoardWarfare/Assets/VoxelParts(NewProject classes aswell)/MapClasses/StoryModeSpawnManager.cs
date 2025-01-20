using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryModeSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnSpot
    {
        public Transform spotTransform; // Position and rotation of the spawn spot
        public bool isOccupied = false; // Tracks if the spot is already used
    }

    [System.Serializable]
    public class MobWavePreset
    {
        public string SceneName; // Scene name to identify the wave
        public int GroundMobs; // Number of ground mobs
        public int HeavyMobs; // Number of heavy mobs
        public int AirMobs; // Number of air mobs
        public int BossMobs; // Number of boss mobs
    }

    // Spawn spots for mobs
    public List<SpawnSpot> MobSpots = new List<SpawnSpot>();
    public List<MobWavePreset> WavePresets = new List<MobWavePreset>(); // Preset waves for mobs

    public GameObject GroundMobPrefab;
    public GameObject HeavyMobPrefab;
    public GameObject AirMobPrefab;
    public GameObject BossMobPrefab; // Boss prefab

    private List<GameObject> spawnedMobs = new List<GameObject>();

    public Player player; // Reference to the Player class

    private void Start()
    {
        Debug.Log("StoryModeSpawnManager Start called.");

        if (GroundMobPrefab == null || HeavyMobPrefab == null || AirMobPrefab == null || BossMobPrefab == null)
            Debug.LogError("One or more mob prefabs are not assigned in the Inspector.");

        // Find all spawn spots tagged with "MobSpawn"
        FindMobSpots();

        // Randomly pick and execute a wave
        ExecuteRandomWave();
        StartCoroutine(CheckMapForMobs());
    }

    private void FindMobSpots()
    {
        GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag("MobSpawn");

        foreach (GameObject spawnObj in spawnObjects)
        {
            SpawnSpot spot = new SpawnSpot
            {
                spotTransform = spawnObj.transform
            };
            MobSpots.Add(spot);
        }

        if (MobSpots.Count == 0)
        {
            Debug.LogError("No MobSpawn spots found in the scene.");
        }
        else
        {
            Debug.Log($"Found {MobSpots.Count} MobSpawn spots.");
        }
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

                ExecuteRandomWave(); // Spawn a new random wave
            }

            // Wait before the next check
            yield return new WaitForSeconds(1f);
        }
    }

    private void ClearSpawnedObjects()
    {
        // Destroy all mobs
        foreach (GameObject mob in spawnedMobs)
        {
            if (mob != null) Destroy(mob);
        }
        spawnedMobs.Clear(); // Clear the tracking list for mobs

        // Reset the "isOccupied" flag for all spawn spots
        foreach (SpawnSpot spot in MobSpots)
            spot.isOccupied = false;

        Debug.Log("Cleared all spawned objects and reset spawn spots.");
    }

    public void ExecuteRandomWave()
    {
        // Check if there are any wave presets in the list
        if (WavePresets.Count == 0)
        {
            Debug.LogError("No wave presets available to execute.");
            return;
        }

        // Pick a random wave preset
        int randomIndex = Random.Range(0, WavePresets.Count);
        MobWavePreset selectedPreset = WavePresets[randomIndex];

        Debug.Log($"Executing Random Wave for Scene '{selectedPreset.SceneName}': Ground {selectedPreset.GroundMobs}, Heavy {selectedPreset.HeavyMobs}, Air {selectedPreset.AirMobs}, Boss {selectedPreset.BossMobs}");

        // Spawn mobs for the wave
        SpawnMobs(GroundMobPrefab, selectedPreset.GroundMobs);
        SpawnMobs(HeavyMobPrefab, selectedPreset.HeavyMobs);
        SpawnMobs(AirMobPrefab, selectedPreset.AirMobs);
        SpawnMobs(BossMobPrefab, selectedPreset.BossMobs);
    }

    public void ExecuteWaveByScene(string sceneName)
    {
        // Find the wave preset for the given scene name
        MobWavePreset wavePreset = WavePresets.Find(w => w.SceneName == sceneName);
        if (wavePreset == null)
        {
            Debug.LogError($"Wave preset for scene '{sceneName}' not found!");
            return;
        }

        Debug.Log($"Executing Wave for Scene '{wavePreset.SceneName}': Ground {wavePreset.GroundMobs}, Heavy {wavePreset.HeavyMobs}, Air {wavePreset.AirMobs}, Boss {wavePreset.BossMobs}");

        // Spawn mobs for the wave
        SpawnMobs(GroundMobPrefab, wavePreset.GroundMobs);
        SpawnMobs(HeavyMobPrefab, wavePreset.HeavyMobs);
        SpawnMobs(AirMobPrefab, wavePreset.AirMobs);
        SpawnMobs(BossMobPrefab, wavePreset.BossMobs);
    }

    private void SpawnMobs(GameObject prefab, int count)
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
