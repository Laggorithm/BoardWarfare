using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIUnit : MonoBehaviour
{
    private bool IsWandering;
    private Vector3 desiredPosition;
    private int rotationSpeed = 150;
    private float wanderRange = 15f;
    private int speed;
    private int armor;
    private float Hp;
    private float Dmg = 10;
    private float attackRange;
    private int ActionValue = 2;
    private Transform ChosenEnemyUnit;
    private string unitClass;
    private float spacing;
    private bool hasChosenEnemy = false;
    private Coroutine wanderingCoroutine;

    public GridSpawner gridSpawner;
    private Animator animator;
    private Tile currentTile;

    private List<Transform> detectedEnemies = new List<Transform>(); // New: Stores detected enemies
    private Transform chosenEnemy = null; // New: Holds the enemy with the lowest health


    void Start()
    {
        InitializeUnitStats();
        IsWandering = true;
        animator = GetComponent<Animator>();
        gridSpawner = FindObjectOfType<GridSpawner>();
        spacing = gridSpawner.spacing;
        SetCurrentTile();
        wanderingCoroutine = StartCoroutine(Wandering());
    }

    private void SetCurrentTile()
    {
        Vector2Int gridPosition = new Vector2Int(
            Mathf.FloorToInt(transform.position.x / spacing),
            Mathf.FloorToInt(transform.position.z / spacing)
        );

        currentTile = gridSpawner.GetTileAtPosition(gridPosition);
        if (currentTile != null)
        {
            currentTile.IsOccupied = true;
        }
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
                speed = 7;
                armor = 5;
                Hp = 50;
                Dmg = 8;
                wanderRange = 20f;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out UnitController enemyController))
        {
            if (!detectedEnemies.Contains(other.transform))
            {
                detectedEnemies.Add(other.transform);
                Debug.Log($"Detected enemy: {other.name}");

                // Check if we need to update our target based on health
                UpdateTargetEnemy();
            }
        }
    }

    private void UpdateTargetEnemy()
    {
        // Filter out any enemies that might have been destroyed or are missing their health component
        detectedEnemies = detectedEnemies
            .Where(enemy => enemy != null && enemy.TryGetComponent(out UnitController _))
            .ToList();

        if (detectedEnemies.Count == 0)
        {
            chosenEnemy = null; // No valid enemies
            return;
        }

        // Find the enemy with the lowest health
        chosenEnemy = detectedEnemies
            .OrderBy(enemy => enemy.GetComponent<UnitController>().health)
            .FirstOrDefault();

        if (chosenEnemy != null)
        {
            MoveToEnemyTile(chosenEnemy);
        }
    }

    private void MoveToEnemyTile(Transform enemy)
    {
        Vector2Int enemyPosition = new Vector2Int(
            Mathf.FloorToInt(enemy.position.x / spacing),
            Mathf.FloorToInt(enemy.position.z / spacing)
        );

        Tile targetTile = GetClosestUnoccupiedTile(enemyPosition);
        if (targetTile != null)
        {
            desiredPosition = targetTile.Position;
            desiredPosition.y = 5f; // Fix Y position to 5

            targetTile.IsOccupied = true;
            if (currentTile != null) currentTile.IsOccupied = false;

            StopCoroutine(wanderingCoroutine); // Stop wandering behavior while moving to enemy
            StartCoroutine(MoveToTargetPosition());
        }
    }


    private IEnumerator MoveToTargetPosition()
    {
        while (Vector3.Distance(transform.position, desiredPosition) > 0.1f)
        {
            animator.SetBool("Walking", true);

            Vector3 direction = desiredPosition - transform.position;
            direction.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);

            // Move to the target position while keeping Y fixed at 5
            Vector3 nextPosition = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * speed);
            nextPosition.y = 5f; // Fix Y position to 5
            transform.position = nextPosition;

            yield return null;
        }

        animator.SetBool("Walking", false);
        currentTile = gridSpawner.GetTileAtPosition(new Vector2Int(
            Mathf.FloorToInt(desiredPosition.x / spacing),
            Mathf.FloorToInt(desiredPosition.z / spacing)
        ));

        // Face the enemy upon reaching the destination
        if (chosenEnemy != null)
        {
            Vector3 enemyDirection = chosenEnemy.position - transform.position;
            enemyDirection.y = 0;
            transform.rotation = Quaternion.LookRotation(enemyDirection);
            yield return new WaitForSeconds(2);
            animator.SetTrigger("Aiming");
            yield return new WaitForSeconds(1);

        }
    }



    private IEnumerator Wandering()
    {
        while (IsWandering)
        {
            List<Vector2Int> possiblePositions = new List<Vector2Int>();
            Vector2Int currentGridPosition = new Vector2Int(
                Mathf.FloorToInt(transform.position.x / spacing),
                Mathf.FloorToInt(transform.position.z / spacing)
            );

            for (int x = -2; x <= 2; x++)
            {
                for (int z = -2; z <= 2; z++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(z) <= 2)
                    {
                        Vector2Int newPos = currentGridPosition + new Vector2Int(x, z);
                        Tile tile = gridSpawner.GetTileAtPosition(newPos);

                        if (tile != null && !tile.IsOccupied)
                        {
                            possiblePositions.Add(newPos);
                        }
                    }
                }
            }

            if (possiblePositions.Count > 0)
            {
                Vector2Int chosenPosition = possiblePositions[Random.Range(0, possiblePositions.Count)];
                Tile targetTile = gridSpawner.GetTileAtPosition(chosenPosition);

                if (targetTile != null)
                {
                    targetTile.IsOccupied = true;
                    if (currentTile != null) currentTile.IsOccupied = false;

                    desiredPosition = targetTile.Position;
                    desiredPosition.y = 5f; // Fixing Y position to 5f

                    while (Vector3.Distance(transform.position, desiredPosition) > 0.1f)
                    {
                        animator.SetBool("Walking", true);

                        Vector3 direction = desiredPosition - transform.position;
                        direction.y = 0;
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);
                        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * speed);

                        yield return null;
                    }

                    animator.SetBool("Walking", false);
                    currentTile = targetTile;
                }
            }

            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
    }

    private void OnDestroy()
    {
        // When the unit is destroyed or removed, mark its last occupied tile as unoccupied
        if (currentTile != null)
        {
            currentTile.IsOccupied = false;
        }
    }



    private Tile GetClosestUnoccupiedTile(Vector2Int targetPosition)
    {
        Tile closestTile = null;
        float closestDistance = Mathf.Infinity;

        // Check tiles around the target position
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector2Int checkPosition = new Vector2Int(targetPosition.x + x, targetPosition.y + z);
                Tile tile = gridSpawner.GetTileAtPosition(checkPosition);
                if (tile != null && !tile.IsOccupied)
                {
                    float distance = Vector2Int.Distance(targetPosition, checkPosition);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTile = tile;
                    }
                }
            }
        }

        return closestTile;
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
        // Handle death logic
        // animator.SetTrigger("Die");
        Debug.Log($"{gameObject.name} has been defeated.");

        // Optionally add a delay before destroying
        Destroy(gameObject, 2f);
    }
}
