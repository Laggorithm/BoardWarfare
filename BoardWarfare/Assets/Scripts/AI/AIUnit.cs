using System.Collections;
using UnityEngine;

public class AIUnit : MonoBehaviour
{
    private bool IsWandering;  // Determine if the unit is in wandering or combat state
    private Vector3 desiredPosition;  // The position the unit moves towards
    private int rotationSpeed = 150;    // Speed of the unit rotation towards the desired position
    private float wanderRange = 15f;   // Range within which the unit can wander
    private int speed;             // Speed of the unit movement
    private int armor;
    private float Hp;
    private float Dmg = 10;
    private float attackRange;
    private int ActionValue = 2;
    private Transform ChosenEnemyUnit;
    private string unitClass;

    private bool hasChosenEnemy = false; // Flag to check if an enemy is already chosen

    private Coroutine wanderingCoroutine;

    // Animator reference
    private Animator animator;

    void Start()
    {
        InitializeUnitStats();
        Debug.ClearDeveloperConsole();
        IsWandering = true;

        // Get the Animator component
        animator = GetComponent<Animator>();

        StartCoroutine(Wandering());   // Start the wandering coroutine when the game starts
    }

    void Update()
    {
        if (ActionValue == 0)
        {
            Idle();
        }
    }

    private void InitializeUnitStats()
    {
        // Initialize stats based on the unit's class
        unitClass = gameObject.tag; // Assuming the class is defined by the GameObject's tag

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
                speed = 7; // Air units are faster
                armor = 5; // Less armor
                Hp = 50; // Lower health
                Dmg = 8; // Lower damage
                wanderRange = 20f; // Larger wander range for Air units
                attackRange = 35f;
                break;
            case "Heavy":
                speed = 3; // Heavy units are slower
                armor = 25; // More armor
                Hp = 200; // More health
                Dmg = 50; // More damage
                attackRange = 15f;
                break;
            default:
                Debug.LogWarning("Unit class not recognized. Default stats applied.");
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
            float randomX = Random.Range(-wanderRange, wanderRange);
            float randomZ = Random.Range(-wanderRange, wanderRange);
            desiredPosition = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
            Debug.Log("New desired position: " + desiredPosition);

            while (Vector3.Distance(transform.position, desiredPosition) > 0.1f)
            {
                if (!IsWandering)
                {
                    yield break; // Stop wandering if transitioning to another state
                }
                animator.SetBool("Walking", true);

                Vector3 direction = desiredPosition - transform.position;
                direction.y = 0;
                Quaternion rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

                transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * speed);

                yield return null;
            }

            animator.SetBool("Walking", false);
            yield return new WaitForSeconds(5f);
        }
    }

    private void Idle()
    {
        StopAllCoroutines();
        // idle animation
        // wait for the end of animation
    }

    private IEnumerator Approaching()
{
    while (hasChosenEnemy) // Continue approaching while an enemy is chosen
    {
        if (ChosenEnemyUnit != null && ActionValue > 0)
        {
            // Randomly choose a point around the enemy unit within a radius of 10 units
            Vector3 randomOffset = new Vector3(
                Random.Range(-10f, 10f),
                0,
                Random.Range(-10f, 10f)
            );

            // Set desiredPosition to be around the enemy unit
            desiredPosition = ChosenEnemyUnit.position + randomOffset;
            Debug.Log("New approach position around enemy: " + desiredPosition);
            animator.SetBool("Walking", true);

            // Move towards the randomly chosen position around the enemy
            while (Vector3.Distance(transform.position, desiredPosition) > 0.1f && hasChosenEnemy)
            {
                // Rotate towards the desired position
                Vector3 direction = desiredPosition - transform.position;
                direction.y = 0;  // Prevent vertical movement

                // Smoothly rotate towards the desired position
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * speed);
                yield return null;  // Wait for the next frame
            }

            // Check if still in range to attack the enemy
            if (ChosenEnemyUnit != null)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, ChosenEnemyUnit.position);
                if (distanceToEnemy <= attackRange)
                {
                    animator.SetBool("Walking", false);
                    yield return new WaitForSeconds(2); // Wait before aiming
                    animator.SetTrigger("Aiming");
                    yield return WaitForAnimationToEnd("Aiming"); // Wait for aiming animation to finish

                    // Calculate the direction towards the enemy
                    Vector3 directionToEnemy = (ChosenEnemyUnit.position - transform.position).normalized;

                    // Calculate the rotation to face the enemy
                    Quaternion lookRotation = Quaternion.LookRotation(directionToEnemy);

                    // Apply an additional rotation to align the right corner of the front with the enemy
                    lookRotation *= Quaternion.Euler(0, 45, 0); // Adjust this value as needed

                    // Smoothly rotate the model to face the enemy with the adjusted rotation
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

                    UnitController unitController = ChosenEnemyUnit.GetComponent<UnitController>();
                    if (unitController != null)
                    {
                        yield return new WaitForSeconds(1); // Optional wait before attacking
                        animator.SetTrigger("Aiming");
                        yield return WaitForAnimationToEnd("Aiming"); // Wait for aiming animation to finish
                        
                        Debug.Log("Enemy in range. Attacking.");
                        unitController.TakeDamage(Dmg);
                        ActionValue = 0; // Deplete action value after attack
                    }
                }
            }
        }

        // If no action value left, wait for recharge or other logic
        if (ActionValue == 0)
        {
            Idle();
            Debug.Log("No ActionValue left. Waiting for recharge.");
            yield break; // Exit the coroutine
        }

        yield return null; // Wait for the next frame to avoid blocking
    }
}

// Coroutine to wait for the animation to finish
private IEnumerator WaitForAnimationToEnd(string animationName)
{
    while (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) &&
           animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
    {
        yield return null; // Wait for the next frame
    }
}



    private void OnTriggerEnter(Collider other)
    {
        // Check if the object has a UnitController component (indicating it's an enemy)
        UnitController enemyUnit = other.GetComponent<UnitController>();

        if (enemyUnit != null && !hasChosenEnemy)
        {
            // Stop wandering if an enemy is detected
            IsWandering = false;

            // Stop the wandering coroutine if it’s running
            if (wanderingCoroutine != null)
            {
                StopCoroutine(wanderingCoroutine);
                wanderingCoroutine = null;
            }

            // Set the chosen enemy and flag it
            ChosenEnemyUnit = other.transform;
            hasChosenEnemy = true; // Mark that an enemy has been chosen
            Debug.Log("Enemy detected: " + ChosenEnemyUnit.name);

            // Start approaching the chosen enemy
            StartCoroutine(Approaching());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the object has a UnitController component (indicating it's an enemy)
        if (other.GetComponent<UnitController>() != null && hasChosenEnemy)
        {
            // Check if the current chosen enemy is the one that just exited
            if (other.transform == ChosenEnemyUnit)
            {
                // Reset the flag and chosen enemy
                hasChosenEnemy = false;
                ChosenEnemyUnit = null;

                // Resume wandering when exiting the trigger range of the chosen enemy
                IsWandering = true;

                // Start the wandering coroutine only if it’s not already running
                if (wanderingCoroutine == null)
                {
                    wanderingCoroutine = StartCoroutine(Wandering());
                }
            }
        }
    }


    public void TakeDamage(float damage)
    {
        float finalDamage = damage * (100 / (100 + armor));
        Hp -= finalDamage;
        if (Hp < 0)
        {
            Destroy(gameObject);
        }
    }
}
