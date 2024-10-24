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



        // You can remove the animation update if no animations are used
        // UpdateAnimationState();
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
            
            // Randomize the X and Z positions within a range from the current position
            float randomX = Random.Range(-wanderRange, wanderRange);
            float randomZ = Random.Range(-wanderRange, wanderRange);

            // Set the desiredPosition as a random spot around the current position within range
            desiredPosition = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
            Debug.Log("New desired position: " + desiredPosition);

            // Move to the desired position
            while (Vector3.Distance(transform.position, desiredPosition) > 0.1f)
            {
                // **Important check**: If the AI is no longer wandering, break out of this loop
                if (!IsWandering)
                {
                    yield break; // Stop wandering if transitioning to another state
                }
                animator.GetBool("Walking");
                animator.SetBool("Walking", true);

                // Rotate towards the desired position
                Vector3 direction = desiredPosition - transform.position;
                direction.y = 0;  // Prevent vertical rotation
                Quaternion rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

                // Move the unit towards the desired position
                transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * speed);

                yield return null;  // Wait until the next frame to continue movement
            }

            // Wait for 5 seconds before choosing a new spot (if still wandering)
            animator.GetBool("Walking");
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
        if (ChosenEnemyUnit != null && ActionValue > 0)
        {
            // Randomly choose a point around the enemy unit within a radius of 10 units
            Vector3 randomOffset = new Vector3(
                Random.Range(-15f, 15f),
                0,
                Random.Range(-15f, 15f)
            );

            // Set desiredPosition to be around the enemy unit
            desiredPosition = ChosenEnemyUnit.position + randomOffset;
            Debug.Log("New approach position around enemy: " + desiredPosition);
            animator.GetBool("Walking");
            animator.SetBool("Walking", true);
            // Move towards the randomly chosen position around the enemy
            while (Vector3.Distance(transform.position, desiredPosition) > 0.1f)
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

            // If within attack range of the enemy after reaching the new position, attack
            float distanceToEnemy = Vector3.Distance(transform.position, ChosenEnemyUnit.position);
            if (distanceToEnemy <= attackRange)
            {
                animator.GetBool("Walking");
                animator.SetBool("Walking", false);
                
                UnitController unitController = ChosenEnemyUnit.GetComponent<UnitController>();
                if (unitController != null)
                {
                    animator.GetBool("Aiming");
                    animator.SetBool("Aiming", true);
                    Debug.Log("Enemy in range. Attacking.");
                    unitController.TakeDamage(Dmg);
                    ActionValue = 0; // Deplete action value after attack
                }
            }

            // If no action value left, wait for recharge or other logic
            if (ActionValue == 0)
            {
                Idle();
                Debug.Log("No ActionValue left. Waiting for recharge.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        UnitController enemyUnit = other.GetComponent<UnitController>();

        if (enemyUnit != null)
        {
            // Stop all coroutines, including wandering
            StopAllCoroutines();  // Make sure no wandering movement continues

            // If there's no chosen enemy yet, or this enemy is closer, set it as the chosen one
            if (ChosenEnemyUnit == null)
            {
                ChosenEnemyUnit = other.transform;
                Debug.Log("New enemy detected: " + ChosenEnemyUnit.name);
            }
            else
            {
                // Calculate the distance to the current chosen enemy
                float currentDistance = Vector3.Distance(transform.position, ChosenEnemyUnit.position);
                // Calculate the distance to the new enemy
                float newEnemyDistance = Vector3.Distance(transform.position, other.transform.position);

                // Choose the closer enemy
                if (newEnemyDistance < currentDistance)
                {
                    ChosenEnemyUnit = other.transform;
                    Debug.Log("Switching to a closer enemy: " + ChosenEnemyUnit.name);
                }
            }

            // Set the wandering state to false to ensure it doesn't wander anymore
            IsWandering = false;

            // Immediately start approaching the chosen enemy
            StartCoroutine(Approaching()); // Transition to approaching state
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<UnitController>() != null)
        {
            IsWandering = true;  // Resume wandering when exiting the trigger
            StartCoroutine(Wandering()); // Restart the wandering behavior
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
