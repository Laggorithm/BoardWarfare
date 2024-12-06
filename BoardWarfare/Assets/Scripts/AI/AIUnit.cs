using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIUnit : MonoBehaviour
{
    private Queue<IEnumerator> actionQueue = new Queue<IEnumerator>();
    private bool isPerformingAction = false; // Tracks if an action is being executed
    private bool isAttacking = false;
    private Vector3 desiredPosition;
    private int rotationSpeed = 150;
    private float wanderRange = 15f;
    private int speed;
    private float attackRange;
    private float Dmg;
    private Transform ChosenEnemyUnit;
    private Transform TargetWallShort;
    private string unitClass;
    private float health;
    public float ArmorStat;
    public float ArmorToughness;
    private float critRate;
    private float critDamage;
    private NavMeshAgent navMeshAgent;
    private GameObject player;

    void Start()
    {

        player = FindObjectOfType<Movement>().gameObject;

        navMeshAgent = GetComponent<NavMeshAgent>();
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        agent.radius = 0.5f; // Adjust the radius based on your unit size
        agent.avoidancePriority = Random.Range(0, 99); // Randomize or set priorities
                                                       // Random priority between 1 and 100
        InitializeUnitStats();
        StartCoroutine(CheckConditions()); // Periodically check for walls or enemies
    }

    private void InitializeUnitStats()
    {
        unitClass = gameObject.tag;
        switch (unitClass)
        {
            case "Ground":
                Dmg = 40;
                critDamage = 20;
                critRate = 30;
                health = 100;
                speed = 5;
                attackRange = 30f;
                break;
            case "Air":
                Dmg = 30;
                critDamage = 10;
                critRate = 40;
                health = 60;
                speed = 10;
                attackRange = 35f;
                break;
            case "Heavy":
                Dmg = 60;
                critDamage = 70;
                critRate = 30;
                health = 250;
                speed = 3;
                attackRange = 30f;
                break;
            default:
                health = 100;
                speed = 5;
                attackRange = 5f;
                break;
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
            // If no enemies are nearby and no actions are pending, enqueue wandering
            else if (actionQueue.Count == 0 && !isPerformingAction)
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
        // Start the walking animation only once when movement begins
        Animator animator = GetComponent<Animator>();
        animator.SetBool("Walking", true);

        // Set the NavMeshAgent's destination to the wall position
        navMeshAgent.SetDestination(wall.position);

        // Wait until the agent reaches the wall
        while (Vector3.Distance(transform.position, wall.position) > 0.5f)
        {
            yield return null;
        }

        Debug.Log("BehindWall");
        // Stop the walking animation when movement finishes
        animator.SetBool("Walking", false);

        // Wait for the cooldown before resuming wandering
        yield return new WaitForSeconds(5f); // 5-second cooldown

        // After cooldown, resume wandering by enqueuing the Wander action
        EnqueueAction(Wander);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Handle WallShort detection
        if (other.CompareTag("WallShort"))
        {
            TargetWallShort = other.transform;
            EnqueueAction(() => MoveToWallShort(other.transform));
        }

        // Handle player detection
        else if (other.CompareTag("Player"))
        {
            // If the detected object is a player, check if it's within range
            if (Vector3.Distance(transform.position, other.transform.position) <= attackRange)
            {
                ChosenEnemyUnit = other.transform;
                EnqueueAction(() => AttackEnemy(ChosenEnemyUnit));
            }
        }
    }

    private IEnumerator AttackEnemy(Transform enemy)
    {
        Animator animator = GetComponent<Animator>();  // Duration to fly up
        float waitTime = 3f;  // Time to wait before flying up

        // Assuming the unit has a Rigidbody component
        Rigidbody rb = GetComponent<Rigidbody>();

        while (enemy != null && Vector3.Distance(transform.position, enemy.position) <= attackRange)
        {
            // Rotate towards the enemy
            Vector3 direction = enemy.position - transform.position;
            direction.y = 0;  // Keep the rotation on the X and Z axes
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

            // Trigger aiming animation
            animator.SetTrigger("Aiming");

            // Aiming time
            yield return new WaitForSeconds(waitTime);

            Movement playerMovement = enemy.GetComponent<Movement>();
            if (playerMovement != null)
            {
                animator.SetBool("Walking", false);
                playerMovement.TakeDamage(Dmg, playerMovement.armor, playerMovement.armorToughness, critRate, critDamage);
            }

            // Wait for attack delay before falling down (simulate the shot delay)
            yield return new WaitForSeconds(2f);  // Adjust based on your needs
        }
    }

    private IEnumerator Wander()
    {
        Animator animator = GetComponent<Animator>();

        // Define the map boundaries and player position
        float minX = 145f, maxX = 180f;
        float minZ = -83f, maxZ = 0f;

        GameObject player = FindObjectOfType<Movement>().gameObject;  // Assuming the player object is set correctly

        // Calculate direction towards the player
        Vector3 playerDirection = player.transform.position - transform.position;

        // Add randomness to the direction within a cone (e.g., +/- 30 degrees)
        float randomAngle = Random.Range(-30f, 30f); // Adjust the range as needed
        playerDirection = Quaternion.Euler(0, randomAngle, 0) * playerDirection;

        // Calculate the desired position
        Vector3 desiredPosition = transform.position + playerDirection.normalized * Random.Range(5f, 10f); // Keep distance from the player

        // Ensure the desired position stays within the map boundaries
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        desiredPosition.z = Mathf.Clamp(desiredPosition.z, minZ, maxZ);

        // Check for nearby units and adjust if necessary
        AvoidNearbyUnits(desiredPosition);

        // Set the NavMeshAgent's destination to the desired position
        navMeshAgent.SetDestination(desiredPosition);

        // Wait until the agent reaches the destination
        animator.SetBool("Walking", true);
        while (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            yield return null;
        }

        // Stop the walking animation when the unit arrives
        animator.SetBool("Walking", false);

        // Wait before wandering again
        yield return new WaitForSeconds(1f); // Pause before the next wander action
    }

    private void AvoidNearbyUnits(Vector3 desiredPosition)
    {
        // Find all units within a certain range
        float avoidRadius = 3f;  // Radius to check for nearby units
        Collider[] nearbyUnits = Physics.OverlapSphere(transform.position, avoidRadius);

        foreach (Collider unit in nearbyUnits)
        {
            if (unit != null && unit.gameObject != gameObject)  // Avoid self
            {
                // Calculate the distance and direction from the nearby unit
                Vector3 directionToUnit = unit.transform.position - transform.position;
                float distance = directionToUnit.magnitude;

                // If the distance is too small, adjust the desired position to avoid overlap
                if (distance < avoidRadius)
                {
                    desiredPosition += directionToUnit.normalized * (avoidRadius - distance);
                }
            }
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
            Debug.Log("Critical Hit! Damage: " + takenDamage); // Debug log for critical hit
        }
        else
        {
            Debug.Log("Normal Hit! Damage: " + takenDamage); // Debug log for normal hit
        }

        // Step 4: Apply damage to the unit's health or other effects here
        health -= takenDamage;

        // Ensure health doesn't go below zero
        health = Mathf.Max(0, health);

        // Optionally, log the final health value here for debugging
        Debug.Log("Unit's Remaining Health: " + health);
    }
}
