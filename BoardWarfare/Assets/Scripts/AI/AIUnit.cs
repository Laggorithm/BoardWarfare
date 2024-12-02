using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
    private List<GameObject> wallShorts = new List<GameObject>();
    private float wallDetectionRange = 100f;
    private float health;
    public float ArmorStat;
    public float ArmorToughness;
    private float critRate;
    private float critDamage;
    void Start()
    {
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

    private static Dictionary<Transform, AIUnit> wallOccupancy = new Dictionary<Transform, AIUnit>();

    private IEnumerator MoveToWallShort(Transform wall)
    {
        // Check if the wall is already occupied by another unit
        if (wallOccupancy.ContainsKey(wall))
        {
            // If another unit is already approaching this wall, avoid it
            Debug.Log("Wall is occupied. Re-routing.");
            EnqueueAction(Wander); // Re-route to wander instead
            yield break;
        }

        // Mark this wall as occupied
        wallOccupancy[wall] = this;

        // Start the walking animation only once when movement begins
        Animator animator = GetComponent<Animator>();
        animator.SetBool("Walking", true);

        // Move towards the wall
        while (Vector3.Distance(transform.position, wall.position) > 0.5f)
        {
            Vector3 direction = wall.position - transform.position;
            direction.y = 0;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

            transform.position = Vector3.MoveTowards(transform.position, wall.position, Time.deltaTime * speed);

            yield return null;
        }

        // Stop the walking animation when movement finishes
        animator.SetBool("Walking", false);

        // Mark the wall as unoccupied after reaching it
        wallOccupancy.Remove(wall);

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

        // Handle collision with other types of objects (WallTall, WallBreakable, etc.)
        else
        {
            switch (other.tag)
            {
                case "WallTall":
                    // Handle WallTall behavior
                    break;

                case "WallBreakable":
                    // Handle WallBreakable behavior
                    break;

                // Additional cases can be added for more object types
                default:
                    Debug.Log("Unknown object detected: " + other.tag);
                    break;
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
            

            // Aiming time
            yield return new WaitForSeconds(waitTime);


            
            Movement playerMovement = enemy.GetComponent<Movement>();
            if (playerMovement != null)
            {
                animator.SetBool("Walking", false);
                animator.SetTrigger("Aiming");
                playerMovement.TakeDamage(Dmg, playerMovement.armor, playerMovement.armorToughness, critRate, critDamage);
            }

            // Wait for attack delay before falling down (simulate the shot delay)
            yield return new WaitForSeconds(2f);  // Adjust based on your needs
        }
    }







    private IEnumerator Wander()
    {
        // Wander in a random direction but check for teammates
        float randomX = Random.Range(-wanderRange, wanderRange);
        float randomZ = Random.Range(-wanderRange, wanderRange);

        desiredPosition = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        // Avoid teammates during wandering, but ensure they don't immediately start spinning
        bool isTooCloseToTeammate = false;
        float checkRadius = 2f; // Radius within which to check for teammates

        // We will check if any teammate is too close (and avoid spinning)
        Collider[] teammates = Physics.OverlapSphere(transform.position, checkRadius, LayerMask.GetMask("Teammates"));
        foreach (Collider teammate in teammates)
        {
            if (teammate.gameObject != gameObject) // Ignore self
            {
                // If a teammate is too close, avoid collision by adjusting the wander position
                isTooCloseToTeammate = true;
                break;
            }
        }

        if (isTooCloseToTeammate)
        {
            // Adjust desired position if a teammate is too close (reroute)
            randomX = Random.Range(-wanderRange, wanderRange);
            randomZ = Random.Range(-wanderRange, wanderRange);
            desiredPosition = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
        }

        // Move towards the desired position
        while (Vector3.Distance(transform.position, desiredPosition) > 0.1f)
        {
            // Calculate direction and rotate smoothly
            Vector3 direction = desiredPosition - transform.position;
            direction.y = 0; // Keep the AI level to the ground
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