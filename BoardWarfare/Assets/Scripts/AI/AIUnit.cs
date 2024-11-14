using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIUnit : MonoBehaviour
{
    private bool IsWandering;
    private Vector3 desiredPosition;
    private int rotationSpeed = 150;
    private int speed;
    private int armor;
    private float Hp;
    private float Dmg = 10;
    private float attackRange;
    private int ActionValue = 2;
    public int cost;
    private Transform ChosenEnemyUnit;
    private string unitClass;
    private float spacing;
    private bool hasChosenEnemy = false;
    private Coroutine wanderingCoroutine;

    public GridSpawner gridSpawner;
    private Animator animator;
    private GridStat currentTile;

    private List<Transform> detectedEnemies = new List<Transform>();
    private Transform chosenEnemy = null;

    private List<GameObject> currentPath = new List<GameObject>();
    private int pathIndex = 0;
    private bool isMoving = false;

    private float wanderCooldown = 3f; // Cooldown time after reaching a tile

    void Start()
    {
        InitializeUnitStats();
        IsWandering = true;
        animator = GetComponent<Animator>();
        gridSpawner = FindObjectOfType<GridSpawner>();

        SetCurrentTile();
        if (currentTile != null)
        {
            spacing = currentTile.spacing;
        }

        StartWandering();
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
            if (!currentTile.IsOccupied)
            {
                currentTile.IsOccupied = true;
            }
        }
        else
        {
            Debug.LogError($"Tile at position {gridPosition} is null! Check grid generation.");
        }
    }

    void Update()
    {
        if (gridSpawner.gridArray == null || gridSpawner.gridArray.Length == 0)
        {
            Debug.LogWarning("Grid not initialized yet, skipping tile check.");
            return;
        }

        SetCurrentTile();

        if (chosenEnemy != null)
        {
            ApproachEnemy();
        }
        else if (!isMoving)
        {
            StartWandering();
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

    private void StartWandering()
    {
        if (wanderingCoroutine == null)
        {
            wanderingCoroutine = StartCoroutine(WanderRoutine());
        }
    }

    private IEnumerator WanderRoutine()
    {
        while (chosenEnemy == null)
        {
            Vector2Int randomDirection = GetRandomDirection();
            Vector2Int targetPosition = new Vector2Int(
                currentTile.x + randomDirection.x,
                currentTile.y + randomDirection.y
            );

            GridStat targetTile = gridSpawner.GetTileAtPosition(targetPosition);

            if (targetTile != null && !targetTile.IsOccupied)
            {
                MoveToTile(targetTile);
                yield return new WaitForSeconds(wanderCooldown);
            }
        }
    }

    private Vector2Int GetRandomDirection()
    {
        Vector2Int[] directions = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };
        return directions[Random.Range(0, directions.Length)];
    }

    private void MoveToTile(GridStat targetTile)
    {
        List<GameObject> path = gridSpawner.GetPath(
            currentTile.x, currentTile.y, targetTile.x, targetTile.y);

        if (path != null && path.Count > 0)
        {
            currentPath = path;
            pathIndex = 0;
            isMoving = true;
            StartCoroutine(FollowPath());
        }
    }

    private IEnumerator FollowPath()
    {
        while (pathIndex < currentPath.Count)
        {
            GameObject target = currentPath[pathIndex];
            Vector3 startPosition = transform.position;
            Vector3 endPosition = new Vector3(
                target.transform.position.x,
                transform.position.y,
                target.transform.position.z
            );

            float journey = 0f;
            while (journey < 1f)
            {
                journey += Time.deltaTime * speed;
                transform.position = Vector3.Lerp(startPosition, endPosition, journey);
                yield return null;
            }

            pathIndex++;
        }

        isMoving = false;
        currentTile.IsOccupied = false;
        //currentTile = gridSpawner.GetTileAtPosition(new Vector2Int(targetTile.x, targetTile.y));
        currentTile.IsOccupied = true;
    }

    private void ApproachEnemy()
    {
        if (chosenEnemy == null) return;

        Vector2Int targetPosition = new Vector2Int(
            Mathf.FloorToInt(chosenEnemy.position.x / spacing),
            Mathf.FloorToInt(chosenEnemy.position.z / spacing)
        );

        List<GameObject> path = gridSpawner.GetPath(
            currentTile.x, currentTile.y, targetPosition.x, targetPosition.y);

        if (path != null && path.Count > 0)
        {
            currentPath = path;
            pathIndex = 0;
            isMoving = true;
            StartCoroutine(FollowPath());
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

                UpdateTargetEnemy();
            }
        }
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

        if (chosenEnemy != null)
        {
            ApproachEnemy();
        }
    }

    private void OnDestroy()
    {
        if (currentTile != null)
        {
            currentTile.IsOccupied = false;
        }
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
