using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIUnit : MonoBehaviour
{
    private int speed;
    private int armor;
    private float Hp;
    private float Dmg = 10;
    private float attackRange;
    private int ActionValue = 2;
    public int cost;
    private Transform chosenEnemy;
    private string unitClass;
    private float spacing;
    private bool hasChosenEnemy = false;
    private List<GameObject> currentPath;  // List to store the current path
    private int currentPathIndex = 0;      // Keeps track of the current tile in the path
    private List<Transform> detectedEnemies = new List<Transform>();
    private List<Tile> availableTiles = new List<Tile>();  // List of available tiles

    void Start()
    {
        InitializeUnitStats();
        TestMovement();
        StartCoroutine(FollowPath());

    }

    private void InitializeUnitStats()
    {
        unitClass = gameObject.tag;

        switch (unitClass)
        {
            case "Ground":
                speed = 5;
                armor = 15;
                Hp = 100;
                Dmg = 40;
                attackRange = 10f;
                cost = 20;
                break;
            case "Air":
                speed = 10;
                armor = 5;
                Hp = 50;
                Dmg = 8;
                cost = 20;
                attackRange = 35f;
                break;
            case "Heavy":
                speed = 3;
                armor = 25;
                Hp = 200;
                Dmg = 50;
                attackRange = 15f;
                cost = 40;
                break;
            default:
                speed = 5;
                armor = 10;
                Hp = 50;
                Dmg = 5;
                attackRange = 5f;
                cost = 20;
                break;
        }
    }

    void Update()
    {
        if (chosenEnemy != null)
        {
            UpdateTargetEnemy();

        }

    }

    public void MoveAlongPath(List<GameObject> path)
    {
        // Get the available tiles from the grid
        availableTiles = GetAvailableTiles();

        currentPath = path;
        currentPathIndex = 0;

        // Start the movement coroutine
        StartCoroutine(FollowPath());
    }

    public void TestMovement()
    {
        // Get the current position of the unit
        Vector3 currentPosition = transform.position;

        // Define a path with one step at a time
        List<GameObject> testPath = new List<GameObject>();

        // Create the initial tile (starting position)
        GameObject tile1 = new GameObject("Tile1");
        tile1.transform.position = currentPosition;

        // Randomly choose a direction and distance to move (10 units)
        Vector3 randomMovement = GetRandomMovement(currentPosition);

        // Create a second tile based on the random movement
        GameObject tile2 = new GameObject("Tile2");
        tile2.transform.position = randomMovement;

        // The final destination after moving 10 units in a random direction
        GameObject finalTile = new GameObject("FinalTile");
        finalTile.transform.position = GetRandomMovement(tile2.transform.position);

        // Add tiles to the path
        testPath.Add(tile1);
        testPath.Add(tile2);
        testPath.Add(finalTile);

        // Call the method to move the unit along this path
        MoveAlongPath(testPath);
    }


    private Vector3 GetRandomMovement(Vector3 currentPosition)
    {
        // Randomly choose the direction and distance (10 units)
        int direction = UnityEngine.Random.Range(0, 4); // 0 - Right, 1 - Left, 2 - Forward, 3 - Backward

        switch (direction)
        {
            case 0: // Move right along the X-axis by 10 units
                return new Vector3(currentPosition.x + 10, currentPosition.y, currentPosition.z);
            case 1: // Move left along the X-axis by 10 units
                return new Vector3(currentPosition.x - 10, currentPosition.y, currentPosition.z);
            case 2: // Move forward along the Z-axis by 10 units
                return new Vector3(currentPosition.x, currentPosition.y, currentPosition.z + 10);
            case 3: // Move backward along the Z-axis by 10 units
                return new Vector3(currentPosition.x, currentPosition.y, currentPosition.z - 10);
            default:
                return currentPosition;
        }
    }


    private IEnumerator FollowPath()
    {
        // Ensure there is a valid path to follow
        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogError("No path to follow!");
            yield break;
        }

        // While there are tiles in the path
        while (currentPathIndex < currentPath.Count)
        {
            GameObject currentTile = currentPath[currentPathIndex];
            Vector3 targetPosition = currentTile.transform.position;
            targetPosition.y = transform.position.y;  // Keep the unit's y position fixed

            // Clamp the target position to restrict movement within bounds
            targetPosition.x = Mathf.Clamp(targetPosition.x, 0f, 90f);
            targetPosition.z = Mathf.Clamp(targetPosition.z, 0f, 90f);

            // Move the unit towards the target position (tile position), restricted to one axis at a time
            Vector3 moveDirection = targetPosition - transform.position;
            float step = speed * Time.deltaTime;

            // Start walking animation
            GetComponent<Animator>().SetBool("Walking", true);

            // Rotate smoothly towards the target direction before moving
            if (Mathf.Abs(moveDirection.x) > 0.1f)
            {
                Vector3 targetDirection = new Vector3(targetPosition.x, transform.position.y, transform.position.z);
                // Rotate the unit to face the target direction on X axis
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection - transform.position);

                // Smooth rotation
                while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
                    yield return null;  // Wait until the next frame
                }

                // Once rotation is complete, move along the X axis
                while (Mathf.Abs(transform.position.x - targetPosition.x) > 0.1f)
                {
                    transform.position = new Vector3(
                        Mathf.MoveTowards(transform.position.x, targetPosition.x, step),
                        transform.position.y,  // Keep Y constant
                        transform.position.z
                    );
                    yield return null;  // Wait until the next frame
                }
            }

            // Move unit along the Z axis if it's not done yet
            if (Mathf.Abs(moveDirection.z) > 0.1f)
            {
                Vector3 targetDirection = new Vector3(transform.position.x, transform.position.y, targetPosition.z);
                // Rotate the unit to face the target direction on Z axis
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection - transform.position);

                // Smooth rotation
                while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
                    yield return null;  // Wait until the next frame
                }

                // Once rotation is complete, move along the Z axis
                while (Mathf.Abs(transform.position.z - targetPosition.z) > 0.1f)
                {
                    transform.position = new Vector3(
                        transform.position.x,  // Keep X constant
                        transform.position.y,  // Keep Y constant
                        Mathf.MoveTowards(transform.position.z, targetPosition.z, step)
                    );
                    yield return null;  // Wait until the next frame
                }
            }

            // Ensure the unit stops exactly at the target position (tile)
            transform.position = targetPosition;

            // Stop the walking animation when the unit reaches the destination or is not moving
            GetComponent<Animator>().SetBool("Walking", false);

            // Wait for a second after reaching the tile before moving to the next one
            Debug.Log("Arrived at tile, pausing for 1 second...");
            yield return new WaitForSeconds(1f); // Pause for 1 second

            // Once the unit reaches the current tile, move to the next tile in the path
            currentPathIndex++;
        }

        // Once the unit has completed the path, you can add any additional logic here (e.g., stop movement, change state)
        Debug.Log("Reached the destination!");
    }


    private List<Tile> GetAvailableTiles()
    {
        // This method would return the list of available tiles (tiles not occupied)
        List<Tile> availableTilesList = new List<Tile>();

        // Here you would loop through the grid's tiles and check if each tile is not occupied
        // (For example, based on a GridSpawner instance or GridStat objects)
        foreach (Tile tile in availableTiles)
        {
            if (!tile.IsOccupied)
            {
                availableTilesList.Add(tile);
            }
        }

        return availableTilesList;
    }





    private void UpdateTargetEnemy()
    {
        detectedEnemies = detectedEnemies
            .Where(enemy => enemy != null && enemy.TryGetComponent(out UnitController _))
            .ToList();

        if (detectedEnemies.Count == 0)
        {
            chosenEnemy = null;
            return;
        }

        chosenEnemy = detectedEnemies
            .OrderBy(enemy => enemy.GetComponent<UnitController>().health)
            .FirstOrDefault();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out UnitController enemyController))
        {
            if (!detectedEnemies.Contains(other.transform))
            {
                detectedEnemies.Add(other.transform);
                Debug.Log($"Detected enemy: {other.name}");

                UpdateTargetEnemy();
            }
        }
    }

    private void OnDestroy()
    {
        // Handle tile clearing logic here if necessary
    }

    public void TakeDamage(float damage)
    {
        Hp -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage, HP remaining: {Hp}");

        if (Hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has been defeated.");
        Destroy(gameObject, 2f);
    }
}