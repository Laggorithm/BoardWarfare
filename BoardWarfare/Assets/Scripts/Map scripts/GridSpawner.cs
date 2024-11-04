using UnityEngine;
using System.Collections.Generic;

public class Tile
{
    public GameObject Block { get; set; } // Reference to the block GameObject
    public bool IsOccupied { get; set; } // Indicates if the tile is occupied

    public Vector3 Position { get; set; }

    public Tile(GameObject block)
    {
        Block = block;
        IsOccupied = false; // Initially, the tile is not occupied
        Position = block.transform.position;
    }
}


public class GridSpawner : MonoBehaviour
{
    public GameObject blockPrefab;  // Prefab for each block
    public int gridSize = 16;       // Grid size (16x16)
    public float spacing = 1.5f;    // Spacing between blocks, adjust based on prefab size

    public Dictionary<Vector2Int, Tile> gridPositions = new Dictionary<Vector2Int, Tile>();

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
                Vector3 spawnPosition = new Vector3(x * spacing, 0, z * spacing);
                GameObject block = Instantiate(blockPrefab, spawnPosition, Quaternion.identity, transform);

                Vector2Int gridCoord = new Vector2Int(x, z);
                gridPositions[gridCoord] = new Tile(block);

                block.name = $"Block_{x}_{z}";
            }
        }
    }

    public Tile GetTileAtPosition(Vector2Int gridCoord)
    {
        gridPositions.TryGetValue(gridCoord, out Tile tile);
        return tile;
    }
}
