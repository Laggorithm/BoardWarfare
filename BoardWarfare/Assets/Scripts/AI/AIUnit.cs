using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AIUnit : MonoBehaviour
{
    private Queue<IEnumerator> actionQueue = new Queue<IEnumerator>();
    private bool isPerformingAction = false; // Tracks if an action is being executed

    private Vector3 desiredPosition;
    private int rotationSpeed = 150;
    private float wanderRange = 15f;
    private int speed;
    private float attackRange;
    private float Dmg = 10;
    private Transform ChosenEnemyUnit;
    private Transform TargetWallShort;
    private string unitClass;
    private List<GameObject> wallShorts = new List<GameObject>();
    private float wallDetectionRange = 100f;
    private float health;
    public float ArmorStat;
    public float ArmorToughness;
    private float critRate;
    private float critDamage;

    private List<AIUnit> teammatesInRange = new List<AIUnit>(); // List of nearby teammates
    private float detectionRange = 20f;  // Range to check for teammates

    void Start()
    {
        InitializeUnitStats();
        CacheWallShorts();
        StartCoroutine(DelayedWallCheck()); // Start the delayed check
    }

    private IEnumerator DelayedWallCheck()
    {
        // Wait for 1 second before starting to check walls
        yield return new WaitForSeconds(1f);

        // Now start checking conditions (which includes walls)
        StartCoroutine(CheckConditions());
    }

    private void InitializeUnitStats()
    {
        unitClass = gameObject.tag;
        switch (unitClass)
        {
            case "Ground":
                critDamage = 20;
                critRate = 30;
                health = 100;
                speed = 5;
                attackRange = 30f;
                break;
            case "Air":
                critDamage = 10;
                critRate = 40;
                health = 60;
                speed = 10;
                attackRange = 35f;
                break;
            case "Heavy":
                critDamage = 70;
                critRate = 30;
                health = 250;
                speed = 3;
                attackRange = 45f;
                break;
            default:
                health = 100;
                speed = 5;
                attackRange = 5f;
                break;
        }
    }

    private void CacheWallShorts()
    {
        wallShorts.Clear();
        GameObject[] walls = GameObject.FindGameObjectsWithTag("WallShort");
        foreach (GameObject wall in walls)
        {
            wallShorts.Add(wall);
        }

        // Debug log to check the wall list
        if (wallShorts.Count == 0)
        {
            Debug.LogWarning("No walls found in the scene with 'WallShort' tag.");
        }
        else
        {
            Debug.Log("Found " + wallShorts.Count + " walls in the scene.");
        }
    }

    private IEnumerator CheckConditions()
    {
        while (true)
        {
            // Check for nearby enemies
            if (CheckForEnemies(out Transform enemy))
            {
                ChosenEnemyUnit = enemy;
                EnqueueAction(() => AttackEnemy(enemy));
            }
            // Check for nearby walls
            else if (CheckForWalls(out Transform wall))
            {
                TargetWallShort = wall;
                EnqueueAction(() => MoveToWallShort(wall));
            }
            // If no actions are pending, wander
            else if (actionQueue.Count == 0)
            {
                EnqueueAction(Wander);
            }

            yield return new WaitForSeconds(2f); // Re-evaluate conditions periodically
        }
    }

    private bool CheckForEnemies(out Transform nearestEnemy)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = player.transform;
            }
        }

        return nearestEnemy != null && nearestDistance <= attackRange;
    }

    private bool CheckForWalls(out Transform nearestWall)
    {
        nearestWall = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject wall in wallShorts)
        {
            float distance = Vector3.Distance(transform.position, wall.transform.position);
            if (distance < nearestDistance && distance <= wallDetectionRange)
            {
                nearestDistance = distance;
                nearestWall = wall.transform;
            }
        }

        return nearestWall != null;
    }

    private void EnqueueAction(System.Func<IEnumerator> action)
    {
        actionQueue.Enqueue(action());
        if (!isPerformingAction)
        {
            StartCoroutine(ExecuteActions());
        }
    }

    private IEnumerator ExecuteActions()
    {
        isPerformingAction = true;
        while (actionQueue.Count > 0)
        {
            yield return StartCoroutine(actionQueue.Dequeue());
        }
        isPerformingAction = false;
    }

    private IEnumerator MoveToWallShort(Transform wall)
    {
        while (Vector3.Distance(transform.position, wall.position) > 0.5f)
        {
            Vector3 direction = wall.position - transform.position;
            direction.y = 0;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

            GetComponent<Animator>().SetBool("Walking", true);
            transform.position = Vector3.MoveTowards(transform.position, wall.position, Time.deltaTime * speed);

            yield return null;
        }

        GetComponent<Animator>().SetBool("Walking", false);
    }

    private IEnumerator AttackEnemy(Transform enemy)
    {
        while (enemy != null && Vector3.Distance(transform.position, enemy.position) <= attackRange)
        {
            Vector3 direction = enemy.position - transform.position;
            direction.y = 0;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

            Debug.Log("Attacking enemy.");
            GetComponent<Animator>().SetTrigger("Aiming");
            Movement playerMovement = enemy.GetComponent<Movement>();

            if (playerMovement != null)
            {
                // Call TakeDamage with the necessary parameters, passing armor from the player's Movement
                playerMovement.TakeDamage(Dmg, playerMovement.armor, playerMovement.armorToughness, critRate, critDamage);
            }

            yield return new WaitForSeconds(2f); // Simulate attack delay
        }
    }

    private IEnumerator Wander()
    {
        float randomX = Random.Range(-wanderRange, wanderRange);
        float randomZ = Random.Range(-wanderRange, wanderRange);

        desiredPosition = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
        while (Vector3.Distance(transform.position, desiredPosition) > 0.1f)
        {
            Vector3 direction = desiredPosition - transform.position;
            direction.y = 0;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

            GetComponent<Animator>().SetBool("Walking", true);
            transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * speed);

            yield return null;
        }

        GetComponent<Animator>().SetBool("Walking", false);
        yield return new WaitForSeconds(5f); // Pause before wandering again
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            EnqueueAction(() => JumpOverObstacle(collision.transform));
        }
    }

    private IEnumerator JumpOverObstacle(Transform obstacle)
    {
        Vector3 jumpTarget = transform.position + transform.forward * 2f;
        float jumpHeight = Mathf.Max(obstacle.localScale.y + 1f, 2f);

        float elapsedTime = 0f;
        float duration = 1f;
        Vector3 startPosition = transform.position;
        Vector3 peakPosition = new Vector3(jumpTarget.x, jumpTarget.y + jumpHeight, jumpTarget.z);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            Vector3 currentPos = Vector3.Lerp(startPosition, peakPosition, t);
            currentPos.y = Mathf.Sin(t * Mathf.PI) * jumpHeight + startPosition.y;
            transform.position = currentPos;

            yield return null;
        }
    }

    public void TakeDamage(float damage, float armorStat, float armorToughness, float critRate, float critDamageStat)
    {
        // Step 1: Calculate the initial damage after armor reduction
        int takenDamage = Mathf.Max(0, Mathf.FloorToInt(damage - (armorStat * armorToughness)));

        // Step 2: Check for critical hit based on the player's crit rate
        float critChance = critRate / 10f; // Convert critRate to a percentage (1 to 10 scale)
        int randomValue = Random.Range(0, 10); // Random value between 1 and 10

        // Step 3: Apply crit damage if the random value is within the crit chance
        if (randomValue <= critChance)
        {
            // Apply critical damage
            takenDamage = Mathf.FloorToInt(takenDamage * (critDamageStat / 100f + 1));
            Debug.Log("Critical Hit! Damage: " + takenDamage);
        }
        else
        {
            Debug.Log("Damage: " + takenDamage);
        }

        health -= takenDamage; // Apply damage to health
    }
}
