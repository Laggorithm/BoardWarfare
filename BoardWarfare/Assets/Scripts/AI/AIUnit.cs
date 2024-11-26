using System.Collections;
using UnityEngine;

public class AIUnit : MonoBehaviour
{
    private bool IsWandering; // Determine if the unit is in wandering or combat state
    private bool IsAiming; // Determine if the unit is aiming and cannot move
    private Vector3 desiredPosition; // The position the unit moves towards
    private int rotationSpeed = 150; // Speed of the unit's rotation
    private float wanderRange = 15f; // Range within which the unit can wander
    private int speed; // Speed of the unit movement
    private float attackRange; // Attack range of the unit
    private float Dmg = 10; // Damage dealt by the unit
    private Transform ChosenEnemyUnit; // Target enemy unit
    private string unitClass; // Class of the unit
    private Rigidbody rb;
    void Start()
    {
        IsWandering = true;
        IsAiming = false;
        InitializeUnitStats();
        StartCoroutine(Wandering());
    }

    private void InitializeUnitStats()
    {
        // Initialize stats based on the unit's class
        unitClass = gameObject.tag;

        switch (unitClass)
        {
            case "Ground":
                speed = 5;
                attackRange = 10f;
                break;
            case "Air":
                speed = 10;
                attackRange = 35f;
                break;
            case "Heavy":
                speed = 3;
                attackRange = 15f;
                break;
            default:
                speed = 5;
                attackRange = 5f;
                break;
        }
    }

    private IEnumerator Wandering()
    {
        while (IsWandering && !IsAiming) // Prevent wandering while aiming
        {
            // Randomize a position within the wander range
            float randomX = Random.Range(-wanderRange, wanderRange);
            float randomZ = Random.Range(-wanderRange, wanderRange);

            desiredPosition = new Vector3(
                transform.position.x + randomX,
                transform.position.y,
                transform.position.z + randomZ
            );

            Debug.Log("New desired position: " + desiredPosition);

            // Rotate towards the desired position
            Vector3 direction = desiredPosition - transform.position;
            direction.y = 0; // Prevent vertical rotation
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

            // Move towards the desired position
            while (Vector3.Distance(transform.position, desiredPosition) > 0.1f)
            {
                GetComponent<Animator>().SetBool("Walking", true);
                transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * speed);
                yield return null;
            }
            GetComponent<Animator>().SetBool("Walking", false);
            // Wait for 5 seconds before choosing a new spot
            yield return new WaitForSeconds(5f);
        }
    }

    private IEnumerator Approaching()
    {
        while (ChosenEnemyUnit != null)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, ChosenEnemyUnit.position);

            // Move towards the enemy until within attack range
            while (distanceToEnemy > attackRange && ChosenEnemyUnit != null && !IsAiming)
            {
                Vector3 direction = ChosenEnemyUnit.position - transform.position;
                direction.y = 0; // Prevent vertical rotation
                Quaternion rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

                transform.position = Vector3.MoveTowards(transform.position, ChosenEnemyUnit.position, Time.deltaTime * speed);
                distanceToEnemy = Vector3.Distance(transform.position, ChosenEnemyUnit.position);

                yield return null;
            }

            if (distanceToEnemy <= attackRange && ChosenEnemyUnit != null)
            {
                Debug.Log("Enemy in range. Maintaining focus on target.");
                IsAiming = true;

                // Stop moving and look at the enemy while aiming
                while (distanceToEnemy <= attackRange && ChosenEnemyUnit != null)
                {
                    Vector3 lookDirection = ChosenEnemyUnit.position - transform.position;
                    lookDirection.y = 0; // Prevent vertical rotation
                    Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
                    rb = GetComponent<Rigidbody>();

                    // Freeze position on all axes (X, Y, Z)
                    rb.constraints = RigidbodyConstraints.FreezePosition;
                    // Attack the enemy
                    Debug.Log("Attacking enemy.");
                    GetComponent<Animator>().SetTrigger("Aiming");
                    ChosenEnemyUnit.GetComponent<Movement>().TakeDamage(Dmg);

                    // Wait for 2 seconds between attacks
                    yield return new WaitForSeconds(2f);

                    // Update distance to enemy
                    distanceToEnemy = Vector3.Distance(transform.position, ChosenEnemyUnit.position);
                }

                IsAiming = false; // Reset aiming state
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Movement>() != null && other.transform == ChosenEnemyUnit)
        {
            ChosenEnemyUnit = null;
            IsAiming = false;
            IsWandering = true;

            // Stop approaching coroutine
            StopCoroutine(Approaching());

            // Resume wandering
            StartCoroutine(Wandering());
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        Movement enemyUnit = other.GetComponent<Movement>();

        if (enemyUnit != null)
        {
            StopCoroutine(Wandering());

            if (ChosenEnemyUnit == null)
            {
                ChosenEnemyUnit = other.transform;
            }

            IsWandering = false;
            StartCoroutine(Approaching());
        }
    }

    public void TakeDamage(float damage)
    {
        // Handle receiving damage
        Debug.Log($"Took {damage} damage.");
    }
}
