using UnityEngine;
using System.Collections.Generic;

public class GridSpawner : MonoBehaviour
{
    public GameObject blockPrefab;  // Prefab for each block
    public int gridSize = 16;       // Grid size (16x16)
    public float spacing = 1.5f;    // Spacing between blocks, adjust based on prefab size

    // Dictionary to hold positions of each block for easy access
    public Dictionary<Vector2Int, GameObject> gridPositions = new Dictionary<Vector2Int, GameObject>();
    public Dictionary<Vector2Int, GameObject> occupiedTiles = new Dictionary<Vector2Int, GameObject>();


    void Start()
    {
        SpawnGrid();
    }

    void SpawnGrid()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                // Calculate position for each block
                Vector3 spawnPosition = new Vector3(x * spacing, 0, z * spacing);

                // Instantiate block prefab at calculated position
                GameObject block = Instantiate(blockPrefab, spawnPosition, Quaternion.identity, transform);

                // Store block in dictionary with its grid coordinates as the key
                Vector2Int gridCoord = new Vector2Int(x, z);
                gridPositions[gridCoord] = block;

                // Optional: Set block name for easier identification in hierarchy
                block.name = $"Block_{x}_{z}";

                gridPositions.TryGetValue(gridCoord, out block);
            }
        }
    }

    // Public method to access specific block positions
    public GameObject GetBlockAtPosition(Vector2Int gridCoord)
    {
        if (gridPositions.TryGetValue(gridCoord, out GameObject block))
        {
            return block;
        }
        return null;
    }
}
