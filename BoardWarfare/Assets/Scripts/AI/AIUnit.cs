using System.Collections;
using System.Collections.Generic;
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

    public void EngageEnemy(Transform enemy)
    {
        // Stops wandering behavior
        IsWandering = false;
        if (wanderingCoroutine != null)
        {
            StopCoroutine(wanderingCoroutine);
        }

        ChosenEnemyUnit = enemy;
        hasChosenEnemy = true;
        StartCoroutine(ApproachAndAttackEnemy());
    }

    private IEnumerator ApproachAndAttackEnemy()
    {
        while (hasChosenEnemy && ChosenEnemyUnit != null)
        {
            Vector3 enemyPosition = ChosenEnemyUnit.position;
            Vector3 direction = enemyPosition - transform.position;
            direction.y = 0;

            // Rotate to face the enemy
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);

            // Move towards the enemy if out of attack range
            if (Vector3.Distance(transform.position, enemyPosition) > attackRange)
            {
                animator.SetBool("Walking", true);
                desiredPosition = enemyPosition;
                desiredPosition.y = 5f; // Fixing Y position to 5f
                transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * speed);
            }
            else
            {
                // Attack logic
                animator.SetBool("Walking", false);
                animator.SetTrigger("Attack");

                // Simulate damage (in a real game, you’d call the enemy’s TakeDamage function here)
                Debug.Log($"{gameObject.name} attacks {ChosenEnemyUnit.name} for {Dmg} damage.");

                yield return new WaitForSeconds(1f); // Attack cooldown
            }

            yield return null;
        }

        // When the enemy is defeated or disengaged, resume wandering
        hasChosenEnemy = false;
        IsWandering = true;
        wanderingCoroutine = StartCoroutine(Wandering());
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
        animator.SetTrigger("Die");
        Debug.Log($"{gameObject.name} has been defeated.");

        // Optionally add a delay before destroying
        Destroy(gameObject, 2f);
    }
}
