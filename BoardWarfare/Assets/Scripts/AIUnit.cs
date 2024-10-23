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

    void Start()
    {
        InitializeUnitStats();
        Debug.ClearDeveloperConsole();
        IsWandering = true;
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
                attackRange = 5f;
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
                // Rotate towards the desired position
                Vector3 direction = desiredPosition - transform.position;
                direction.y = 0;  // Prevent vertical rotation
                Quaternion rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

                // Move the unit towards the desired position
                transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * speed);
                yield return null;  // Wait until the next frame to continue movement
            }

            // Wait for 5 seconds before choosing a new spot
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
        // Move to the previously set desired position first
        while (Vector3.Distance(transform.position, desiredPosition) > 0.1f)
        {
            // Rotate towards the desired position
            Vector3 direction = desiredPosition - transform.position;
            direction.y = 0;  // Prevent vertical movement

            // Rotate towards the desired position
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * speed);
            yield return null;  // Wait for the next frame
        }

        // Wait for 2 seconds after stopping
        yield return new WaitForSeconds(2f);

        // Calculate the distance to the enemy now that the unit has stopped
        if (ChosenEnemyUnit != null && ActionValue > 0)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, ChosenEnemyUnit.position);
            UnitController unitController = ChosenEnemyUnit.GetComponent<UnitController>();

            // If the unit is within attack range, attack the enemy
            if (distanceToEnemy <= attackRange)
            {
                // Attack animation
                Debug.Log("Enemy in range. Ready to attack.");
                unitController.TakeDamage(Dmg);
                Debug.Log("Attacked Enemy Unit");
                ActionValue = 0;  // Set action value to 0 after attacking
                yield break;  // Exit the coroutine since no more movement is needed
            }

            // Move towards the enemy if not in range
            while (distanceToEnemy > attackRange && ActionValue > 0)
            {
                // Update the distance after moving
                distanceToEnemy = Vector3.Distance(transform.position, ChosenEnemyUnit.position);
                float moveDistance = Mathf.Min(10f, distanceToEnemy - attackRange);  // Cap movement to 10 units
                Vector3 forwardMovement = transform.forward * moveDistance * Time.deltaTime * speed;

                // Move the unit forward
                transform.position += forwardMovement;

                // Consume one action value for every movement towards the enemy
                ActionValue -= 1;
                Debug.Log($"Moved {moveDistance} units towards enemy. Remaining ActionValue: {ActionValue}");

                // Wait before the next action
                yield return new WaitForSeconds(2f);
            }
        }

        // If we run out of action value
        if (ActionValue == 0)
        {
            Debug.Log("No ActionValue left. Waiting for recharge.");
            // Optionally, handle what happens while the unit waits for more ActionValue
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the other object has the UnitController component
        UnitController enemyUnit = other.GetComponent<UnitController>();

        if (enemyUnit != null)
        {
            StopCoroutine(Wandering());

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

            // Stop wandering and start approaching the chosen enemy
            IsWandering = false;
            StartCoroutine(Approaching()); // Immediately approach the chosen enemy
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<UnitController>() != null)
        {
            IsWandering = true;  // Resume wandering when exiting the trigger
            StartCoroutine(Wandering()); // Restart the wandering coroutine if it was stopped
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
