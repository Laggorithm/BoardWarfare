using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AIUnit : MonoBehaviour
{
    private Queue<IEnumerator> actionQueue = new Queue<IEnumerator>();
    private bool isPerformingAction = false; // Tracks if an action is being executed
    private List<AIUnit> teammatesInRange = new List<AIUnit>();
    private float detectionRange = 10f;
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
    private float communicationRange = 3f;  // Range at which units can communicate
    private float communicationCooldown = 1f;  // Cooldown time after sending/receiving a signal
    private float lastCommunicationTime = 0f;  // Time of last communication check
    private bool isMovingAway = false;  // Flag to determine if the unit is moving away from a teammate
    private int communicationSignal = 1;  // Signal to send to the teammate (1 = continue, 2 = move away)
    private AIUnit closestTeammate = null;  // Reference to the closest teammate within range
    private float idleTimer = 0f;
    private float idleThreshold = 5f; // 5 seconds threshold
    private Rigidbody rb;
    void Start()
    {
        InitializeUnitStats();
        CacheWallShorts();
        StartCoroutine(DelayedWallCheck()); // Start the delayed check

        // Debug: Log all game objects in the scene to check the walls
        GameObject[] allWalls = GameObject.FindGameObjectsWithTag("WallShort");
        foreach (var wall in allWalls)
        {
            Debug.Log("Found wall: " + wall.name + " at position: " + wall.transform.position);
        }
        rb = GetComponent<Rigidbody>();
    }
    private void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;  // Reset velocity to zero
        }
    }
    private AIUnit FindClosestTeammate()
    {
        AIUnit[] allUnits = FindObjectsOfType<AIUnit>();
        AIUnit closest = null;
        float nearestDistance = float.MaxValue;

        foreach (AIUnit unit in allUnits)
        {
            if (unit != this)  // Ignore itself
            {
                float distance = Vector3.Distance(transform.position, unit.transform.position);
                if (distance < nearestDistance && distance <= communicationRange)
                {
                    nearestDistance = distance;
                    closest = unit;
                }
            }
        }

        return closest;
    }
    private void SendCommunicationSignal()
    {
        // If unit is already moving away, send signal "2", otherwise send "1"
        communicationSignal = isMovingAway ? 2 : 1;
    }
    private void ReceiveCommunicationSignal(int signal)
    {
        if (signal == 2 && !isMovingAway)
        {
            // If the received signal is 2, move away from the teammate
            isMovingAway = true;
            StartCoroutine(MoveAwayFromTeammate());
        }
        else if (signal == 1 && isMovingAway)
        {
            // If the received signal is 1 and we're moving away, stop moving away
            isMovingAway = false;
            StopAllCoroutines();  // Stop moving away and resume wandering
            StartCoroutine(Wander());  // Start wandering again
        }
    }
    private IEnumerator MoveAwayFromTeammate()
    {
        // Move away from the teammate until the distance is greater than communicationRange
        while (Vector3.Distance(transform.position, closestTeammate.transform.position) <= communicationRange)
        {
            // Move in the opposite direction from the teammate
            Vector3 direction = transform.position - closestTeammate.transform.position;
            direction.y = 0;  // Ensure the movement is horizontal
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

            transform.position = Vector3.MoveTowards(transform.position, transform.position + direction, Time.deltaTime * speed);

            yield return null;
        }

        // Once the unit has moved away, continue wandering
        isMovingAway = false;
        StartCoroutine(Wander());  // Start wandering again after moving away
    }


    private void CheckAndCommunicateWithTeammate()
    {
        if (Time.time - lastCommunicationTime < communicationCooldown)
            return;  // Avoid checking too often

        // Find the closest teammate within range
        closestTeammate = FindClosestTeammate();

        if (closestTeammate != null && Vector3.Distance(transform.position, closestTeammate.transform.position) <= communicationRange)
        {
            // Send a signal to the closest teammate
            SendCommunicationSignal();

            // Receive the signal and handle movement decision
            ReceiveCommunicationSignal(closestTeammate.communicationSignal);

            lastCommunicationTime = Time.time;  // Update the last communication time
        }
    }
    private void FindTeammatesInRange()
    {
        teammatesInRange.Clear();  // Clear previous teammates

        // Get all AIUnit instances in the scene
        AIUnit[] allUnits = FindObjectsOfType<AIUnit>();

        foreach (AIUnit unit in allUnits)
        {
            if (unit != this)  // Make sure we're not checking the unit itself
            {
                float distance = Vector3.Distance(transform.position, unit.transform.position);

                if (distance <= detectionRange)
                {
                    teammatesInRange.Add(unit);
                }

                // If a teammate is within 3f, cancel current actions and avoid collision
                if (distance <= 3f)
                {
                    StopAllCoroutines();  // Cancel all current coroutines
                    AvoidTeammate(unit);  // Change direction to avoid the teammate
                    break;  // Only need to react to one teammate
                }
            }
        }
    }
    private void AvoidTeammate(AIUnit teammate)
    {
        Vector3 directionAwayFromTeammate = transform.position - teammate.transform.position;
        directionAwayFromTeammate.y = 0; // Ensure we only move on the X-Z plane

        // Randomize direction to move away from the teammate
        Vector3 newDirection = directionAwayFromTeammate.normalized + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        newDirection = newDirection.normalized;

        // Set new desired position based on direction
        desiredPosition = transform.position + newDirection * 5f; // Move 5 units away for now

        // Start moving away from the teammate immediately
        EnqueueImmediateAction(() => MoveToPosition(desiredPosition));

        // After moving away, proceed to continue wandering
        StartCoroutine(WaitAndWander());
    }

    private IEnumerator WaitAndWander()
    {
        // Wait until the unit has moved a sufficient distance away from the teammate
        while (Vector3.Distance(transform.position, desiredPosition) > 0.5f)
        {
            yield return null; // Wait until the unit has finished moving away
        }

        // Once the unit is far enough from the teammate, start wandering
        EnqueueAction(Wander); // Continue wandering after moving away from the teammate
    }



    private void EnqueueImmediateAction(System.Func<IEnumerator> action)
    {
        StartCoroutine(action());  // Execute the action immediately without queuing
    }


    void Update()
    {

        CheckAndCommunicateWithTeammate();
        // Track idle time
        if (Vector3.Distance(transform.position, desiredPosition) <= 0.1f)
        {
            idleTimer += Time.deltaTime;
        }
        else
        {
            idleTimer = 0f;  // Reset timer when moving
        }

        // If the unit has been idle for 5 seconds, start wandering
        if (idleTimer >= idleThreshold && actionQueue.Count == 0)
        {
            EnqueueAction(Wander);
            idleTimer = 0f;  // Reset timer after starting wandering
        }
        if (IsUnitStationary() && !isWandering && Time.time - lastMoveTime >= 5f)
        {
            StartWandering();  // Start wandering after being stationary for 5 seconds
        }
        // Continuously check for teammates (keep your existing logic)
        FindTeammatesInRange();
    }

    private IEnumerator DelayedWallCheck()
    {
        // Wait for 1 second before starting to check walls
        yield return new WaitForSeconds(1f);

        // Log the walls again after the delay to check if they are available by then
        CacheWallShorts();
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

        // Stop the unit's movement and reset the velocity
        GetComponent<Animator>().SetBool("Walking", false);
        StopMovement(); // Stop the unit's velocity
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

        // Stop the unit's movement and reset the velocity
        GetComponent<Animator>().SetBool("Walking", false);
        StopMovement(); // Stop the unit's velocity
        yield return new WaitForSeconds(5f); // Pause before wandering again
    }

    private Vector3 GetRandomWanderingTarget()
    {
        // Generate random offsets based on wanderRange
        float randomX = Random.Range(-wanderRange, wanderRange);
        float randomZ = Random.Range(-wanderRange, wanderRange);

        // Create a random position using the current position as the base
        Vector3 randomTarget = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        return randomTarget;
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
    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

            GetComponent<Animator>().SetBool("Walking", true);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);

            yield return null;
        }

        GetComponent<Animator>().SetBool("Walking", false);
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
            Debug.Log("Critical Hit! Damage: " + takenDamage); // Debug log for critical hit
        }
        else
        {
            Debug.Log("Normal Hit! Damage: " + takenDamage); // Debug log for normal hit
        }

        // Step 4: Apply damage to the unit's health or other effects here
        // Assuming the AIUnit has a health variable
        health -= takenDamage;

        // Ensure health doesn't go below zero
        health = Mathf.Max(0, health);

        // Optionally, log the final health value here for debugging
        Debug.Log("Unit's Remaining Health: " + health);
    }

}