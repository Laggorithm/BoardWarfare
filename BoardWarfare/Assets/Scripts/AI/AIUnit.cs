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

    private Transform chosenEnemy;
    private string unitClass;
    private float spacing;
    private bool hasChosenEnemy = false;
    private List<GameObject> currentPath;  // List to store the current path
    private int currentPathIndex = 0;      // Keeps track of the current tile in the path
    private List<Transform> detectedEnemies = new List<Transform>();
    private List<Tile> availableTiles = new List<Tile>();  // List of available tiles
    float rotationAngle;
    private bool isWandering = false;
    UnitController unitController;
    private Transform chosenWall;  // Reference for the wall unit will approach
    private List<Transform> detectedWalls = new List<Transform>();  // List of walls detected by the unit
    bool BehindWall;
    bool SeeEnemy;
    void Start()
    {
        InitializeUnitStats();
        StartWandering();  // Start the wandering behavior
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

                break;
            case "Air":
                speed = 10;
                armor = 5;
                Hp = 50;
                Dmg = 8;

                attackRange = 35f;
                break;
            case "Heavy":
                speed = 3;
                armor = 25;
                Hp = 200;
                Dmg = 50;
                attackRange = 15f;

                break;
            default:
                speed = 5;
                armor = 10;
                Hp = 50;
                Dmg = 5;
                attackRange = 5f;

                break;
        }
    }

    void Update()
    {
        if (chosenEnemy != null)
        {
            UpdateTargetEnemy();
        }
        switch (ActionValue)
        {
            case (0):
                StopAllCoroutines();
                break;
        }
         
    }

    public void StartWandering()
    {
        isWandering = true;
        StartCoroutine(WanderingRoutine());
    }

    public void StopWandering()
    {
        isWandering = false;
        StopCoroutine(WanderingRoutine());
    }

    private IEnumerator WanderingRoutine()
    {
        while (isWandering)
        {
            Vector3 currentPosition = transform.position;

            // Randomly choose a new target position within 10 units of the current position
            Vector3 targetPosition = GetRandomMovement(currentPosition);
            GameObject targetTile = new GameObject("WanderTile");
            targetTile.transform.position = targetPosition;

            // Set path to the new target position
            currentPath = new List<GameObject> { targetTile };
            currentPathIndex = 0;

            // Move along the path to the target tile
            yield return StartCoroutine(FollowPath());

            // Pause briefly at the target position before selecting a new position
            yield return new WaitForSeconds(1f);
        }
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
        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogError("No path to follow!");
            yield break;
        }

        while (currentPathIndex < currentPath.Count)
        {
            GameObject currentTile = currentPath[currentPathIndex];
            Vector3 targetPosition = currentTile.transform.position;
            targetPosition.y = transform.position.y;  // Keep the unit's y position fixed

            targetPosition.x = Mathf.Clamp(targetPosition.x, 0f, 90f);
            targetPosition.z = Mathf.Clamp(targetPosition.z, 0f, 90f);

            Vector3 moveDirection = targetPosition - transform.position;
            float step = speed * Time.deltaTime;

            GetComponent<Animator>().SetBool("Walking", true);

            if (Mathf.Abs(moveDirection.x) > 0.1f)
            {
                Vector3 targetDirection = new Vector3(targetPosition.x, transform.position.y, transform.position.z);
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection - transform.position);

                while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
                    yield return null;
                }

                while (Mathf.Abs(transform.position.x - targetPosition.x) > 0.1f)
                {
                    transform.position = new Vector3(
                        Mathf.MoveTowards(transform.position.x, targetPosition.x, step),
                        transform.position.y,
                        transform.position.z
                    );
                    yield return null;
                }
            }

            if (Mathf.Abs(moveDirection.z) > 0.1f)
            {
                Vector3 targetDirection = new Vector3(transform.position.x, transform.position.y, targetPosition.z);
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection - transform.position);

                while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
                    yield return null;
                }

                while (Mathf.Abs(transform.position.z - targetPosition.z) > 0.1f)
                {
                    transform.position = new Vector3(
                        transform.position.x,
                        transform.position.y,
                        Mathf.MoveTowards(transform.position.z, targetPosition.z, step)
                    );
                    yield return null;
                }
            }

            transform.position = targetPosition;
            GetComponent<Animator>().SetBool("Walking", false);

            currentPathIndex++;
            Destroy(currentTile);  // Clean up the temporary target tile GameObject
        }
    }

    public void MoveAlongPath(List<GameObject> path)
    {
        // Store the path and start following it from the beginning
        currentPath = path;
        currentPathIndex = 0;

        // Start moving along the path
        StartCoroutine(FollowPath());
    }

    private void SetTargetNearEnemy()
    {
        // Stop any wandering if currently in progress
        isWandering = false;

        // Stop any active coroutines
        StopAllCoroutines();

        if (chosenEnemy == null)
        {
            Debug.LogError("No enemy chosen to target.");
            return;
        }

        // Get the enemy's position
        Vector3 enemyPosition = chosenEnemy.position;

        // Determine a nearby position to move towards (e.g., 10 units to the right of the enemy)
        Vector3 targetPosition = enemyPosition + new Vector3(10, 0, 0);// Adjust this to any side if needed
        // Create a list to store the movement path
        List<Vector3> pathToTarget = new List<Vector3>();

        // Simulate tile-based movement by adding steps towards the target in increments of 10 units
        Vector3 currentPos = transform.position;
        while (Vector3.Distance(currentPos, targetPosition) > 0.1f)
        {
            // Move 10 units in X or Z direction, whichever is closer to the target
            if (Mathf.Abs(targetPosition.x - currentPos.x) > Mathf.Abs(targetPosition.z - currentPos.z))
            {
                currentPos.x = Mathf.MoveTowards(currentPos.x, targetPosition.x, 10);
            }
            else
            {
                currentPos.z = Mathf.MoveTowards(currentPos.z, targetPosition.z, 10);
            }

            // Add the calculated step position to the path
            pathToTarget.Add(currentPos);
        }

        // Convert path positions to GameObjects (temporary objects for movement)
        List<GameObject> pathObjects = pathToTarget.Select(pos =>
        {
            GameObject stepObject = new GameObject("PathStep");
            stepObject.transform.position = pos;
            return stepObject;
        }).ToList();

        // Move along the generated path
        StartCoroutine(FollowPath(pathObjects));

        Debug.Log("Chasing enemy to a position near them.");
    }

    private IEnumerator FollowPath(List<GameObject> path)
    {
        if (path == null || path.Count == 0)
        {
            Debug.LogError("No path to follow!");
            yield break;
        }

        foreach (GameObject currentTile in path)
        {
            Vector3 targetPosition = currentTile.transform.position;
            targetPosition.y = transform.position.y;  // Keep the unit's y position fixed

            Vector3 moveDirection = targetPosition - transform.position;
            float step = speed * Time.deltaTime;

            GetComponent<Animator>().SetBool("Walking", true);

            // Rotate towards the movement direction
            if (moveDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
                    yield return null;
                }
            }

            // Move towards the target position
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
                yield return null;
            }

            transform.position = targetPosition;  // Final position
            GetComponent<Animator>().SetBool("Walking", false);

            // After reaching the destination, turn towards the enemy
            TurnTowardsEnemy(chosenEnemy);

            Destroy(currentTile);  // Clean up the temporary target tile GameObject
        }
        ActionValue = 0;
        Debug.Log(ActionValue); 
        Debug.Log("Reached the target near the enemy.");
        GetComponent<Animator>().SetTrigger("Aiming");
         
    }





    // Function to make the unit turn towards the enemy
    private void TurnTowardsEnemy(Transform enemy)
    {
        if (enemy == null) return;

        // Step 1: Calculate the direction to the enemy
        Vector3 directionToEnemy = enemy.position - transform.position;
        directionToEnemy.y = 0;  // Ignore vertical movement, keep horizontal rotation

        if (tag == "Ground")
        {
            rotationAngle = 50f;
        }
        else { rotationAngle = 0; }
         
        directionToEnemy = Quaternion.Euler(0, rotationAngle, 0) * directionToEnemy; // Apply rotation offset to direction

        // Step 3: Make the unit look towards the new direction
        transform.LookAt(transform.position + directionToEnemy);  // Rotate towards the modified direction


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
        if (other.CompareTag("WallTall") || other.CompareTag("WallShort"))
        {
            if (!detectedWalls.Contains(other.transform))
            {
                detectedWalls.Add(other.transform);
                Debug.Log($"Detected wall: {other.name}");
                StopAllCoroutines();
                UpdateTargetWall();
                SetTargetNearWall();
                BehindWall = true;
                 
            }
        }
        else if (other.TryGetComponent(out UnitController enemyController))
        {
            if (!detectedEnemies.Contains(other.transform))
            {
                detectedEnemies.Add(other.transform);
                Debug.Log($"Detected enemy: {other.name}");

                
                UpdateTargetEnemy();
                
                SeeEnemy = true;

                if (SeeEnemy || BehindWall)
                {
                    GetComponent<Animator>().SetTrigger("Aiming");
                    enemyController.TakeDamage(Dmg);
                }
                else if (SeeEnemy || !BehindWall)
                {
                    SetTargetNearEnemy();
                }
            }
        }

    }

    private void UpdateTargetWall()
    {
        detectedWalls = detectedWalls
            .Where(wall => wall != null)
            .ToList();

        if (detectedWalls.Count == 0)
        {
            chosenWall = null;
            return;
        }

        // If walls are detected, just pick the first one for simplicity or prioritize based on some logic (distance, etc.)
        chosenWall = detectedWalls.FirstOrDefault();
    }

    private void SetTargetNearWall()
{
    // Stop any wandering if currently in progress
    isWandering = false;

    // Stop any active coroutines
    StopAllCoroutines();

    if (chosenWall == null)
    {
        Debug.LogError("No wall chosen to target.");
        return;
    }

    // Get the wall's position
    Vector3 wallPosition = chosenWall.position;

    // Determine a nearby position to move towards (e.g., 10 units in front of the wall)
    Vector3 targetPosition = wallPosition + new Vector3(0, 0, 0);  // Adjust as needed based on wall type

    // Create a list to store the movement path
    List<Vector3> pathToTarget = new List<Vector3>();

    // Simulate tile-based movement by adding steps towards the target in increments of 10 units
    Vector3 currentPos = transform.position;
    while (Vector3.Distance(currentPos, targetPosition) > 0.1f)
    {
        // Move 10 units in X or Z direction, whichever is closer to the target
        if (Mathf.Abs(targetPosition.x - currentPos.x) > Mathf.Abs(targetPosition.z - currentPos.z))
        {
            currentPos.x = Mathf.MoveTowards(currentPos.x, targetPosition.x, 10);
        }
        else
        {
            currentPos.z = Mathf.MoveTowards(currentPos.z, targetPosition.z, 10);
        }

        // Add the calculated step position to the path
        pathToTarget.Add(currentPos);
    }

    // Convert path positions to GameObjects (temporary objects for movement)
    List<GameObject> pathObjects = pathToTarget.Select(pos =>
    {
        GameObject stepObject = new GameObject("PathStep");
        stepObject.transform.position = pos;
        return stepObject;
    }).ToList();

    // Move along the generated path
    StartCoroutine(FollowPath(pathObjects));

    Debug.Log("Approaching wall to a position near it.");
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

    private void Attack(Transform enemy)
    {
        GetComponent<Animator>().SetTrigger("Aiming");
        unitController.TakeDamage(Dmg);
    }
    private void Die()
    {
        Debug.Log($"{gameObject.name} has been defeated.");
        Destroy(gameObject, 2f);
    }
}
