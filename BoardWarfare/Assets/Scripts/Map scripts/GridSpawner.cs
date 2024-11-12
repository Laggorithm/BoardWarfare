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
    public bool findDistance = false;
    public int rows = 10;
    public int columns = 10;
    public int scale = 1;
    public Vector3 leftBottomLocation = new Vector3(0, 0, 0);
    public GameObject[,] gridArray;
    public int startX = 0;
    public int startY = 0;
    public int endX = 2;
    public int endY = 2;
    public List<GameObject> path = new List<GameObject>();
    private void Awake()
    {
        gridArray = new GameObject[rows, columns];
        if (blockPrefab)
            GenerateGrid();
        else print("missing gridprefab, please assign.");
    }
    private void Update()
    {
        if (findDistance)
        {
            SetDistance();
            SetPath();
            findDistance = false;
        }
    }
    void GenerateGrid()
    {
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                GameObject obj = Instantiate(blockPrefab, new Vector3(leftBottomLocation.x + scale * i, leftBottomLocation.y, leftBottomLocation.z + scale * j), Quaternion.identity);
                obj.transform.SetParent(gameObject.transform);
                obj.GetComponent<GridStat>().x = i;
                obj.GetComponent<GridStat>().y = j;
                print("Grid generated");
                gridArray[i, j] = obj;
            }
        }
    }
    void InitialSetUp()
    {
        foreach (GameObject obj in gridArray)
        {
            obj.GetComponent<GridStat>().visited = -1;
        }
        gridArray[startX, startY].GetComponent<GridStat>().visited = 0;
    }
    bool TestDirection(int x, int y, int step, int direction)
    {
        switch (direction)
        {
            case 1:
                if (y + 1 < rows && gridArray[x, y + 1] && gridArray[x, y + 1].GetComponent<GridStat>().visited == step)
                    return true;
                else
                    return false;
            case 2:
                if (y - 1 > -1 && gridArray[x, y - 1] && gridArray[x, y - 1].GetComponent<GridStat>().visited == step)
                    return true;
                else
                    return false;
            case 3:
                if (x + 1 < columns && gridArray[x + 1, y] && gridArray[x + 1, y].GetComponent<GridStat>().visited == step)
                    return true;
                else
                    return false;
            case 4:
                if (x - 1 > -1 && gridArray[x - 1, y] && gridArray[x - 1, y].GetComponent<GridStat>().visited == step)
                    return true;
                else
                    return false;
        }
        return false;
    }
    void SetDistance()
    {
        InitialSetUp();
        int x = startX;
        int y = startY;
        int[] testArray = new int[rows * columns];
        for (int step = 1; step < rows * columns; step++)
        {
            foreach (GameObject obj in gridArray)
            {
                if (obj && obj.GetComponent<GridStat>().visited == step - 1)
                {
                    TestFourDirections(obj.GetComponent<GridStat>().x, obj.GetComponent<GridStat>().y, step);
                }
            }
        }
    }
    void SetVisited(int x, int y, int step)
    {
        if (gridArray[x, y])
        {
            gridArray[x, y].GetComponent<GridStat>().visited = step;
        }
    }
    void SetPath()
    {
        int step;
        int x = endX;
        int y = endY;
        List<GameObject> tempList = new List<GameObject>();
        path.Clear();
        if (gridArray[endX, endY] && gridArray[endX, endY].GetComponent<GridStat>().visited > 0)
        {
            path.Add(gridArray[x, y]);
            step = gridArray[x, y].GetComponent<GridStat>().visited - 1;
        }
        else
        {
            print("Can't reach the desired location");
            return;
        }
        for (int i = step; step > -1; step--)
        {
            if (TestDirection(x, y, step, 1))
            {
                tempList.Add(gridArray[x + 1, y]);
            }
            if (TestDirection(x, y, step, 2))
            {
                tempList.Add(gridArray[x, y + 1]);
            }
            if (TestDirection(x, y, step, 3))
            {
                tempList.Add(gridArray[x, y - 1]);
            }
            if (TestDirection(x, y, step, 4))
            {
                tempList.Add(gridArray[x - 1, y]);
            }
            GameObject tempObj = FindClosest(gridArray[endX, endY].transform, tempList);
            path.Add(tempObj);
            x = tempObj.GetComponent<GridStat>().x;
            y = tempObj.GetComponent<GridStat>().y;
            tempList.Clear();
        }

    }
    void TestFourDirections(int x, int y, int step)
    {
        // Test up (direction 1)
        if (TestDirection(x, y, step - 1, 1))
        {
            SetVisited(x, y + 1, step);
        }
        // Test down (direction 2)
        if (TestDirection(x, y, step - 1, 2))
        {
            SetVisited(x, y - 1, step);
        }
        // Test right (direction 3)
        if (TestDirection(x, y, step - 1, 3))
        {
            SetVisited(x + 1, y, step);
        }
        // Test left (direction 4)
        if (TestDirection(x, y, step - 1, 4))
        {
            SetVisited(x - 1, y, step);
        }
    }

    GameObject FindClosest(Transform targetLocation, List<GameObject> list)
    {
        float currentDistance = scale * rows * columns;
        int indexNumber = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (Vector3.Distance(targetLocation.position, list[i].transform.position) < currentDistance)
            {
                currentDistance = Vector3.Distance(targetLocation.position, list[i].transform.position);
                indexNumber = i;
            }
        }
        return list[indexNumber];
    }

}
