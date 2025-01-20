using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MobBehaviour : MonoBehaviour
{
    private Queue<IEnumerator> actionQueue = new Queue<IEnumerator>();
    private bool isPerformingAction = false; // Tracks if an action is being executed
    private Vector3 desiredPosition;
    private int rotationSpeed = 150;
    private float wanderRange = 15f;
    private int speed;
    private float attackRange;
    private float Dmg;
    private string unitClass;
    private float health;
    private NavMeshAgent navMeshAgent;

    void Start()
    {
        // Initialize stats based on wave difficulty and unit class
        InitializeUnitStats();

        // Initialize the NavMeshAgent for movement
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.radius = 0.5f; // Adjust the radius based on your unit size
        navMeshAgent.avoidancePriority = Random.Range(0, 99); // Randomize or set priorities
        navMeshAgent.updateRotation = false; // Disable automatic rotation to manually handle it

        // Start the wandering behavior
        StartCoroutine(Wander());
    }

    private void InitializeUnitStats()
    {
        unitClass = gameObject.tag;
        switch (unitClass)
        {
            case "Ground":
                Dmg = 40;
                health = 100;
                speed = 5;
                attackRange = 30f;
                break;
            case "Air":
                Dmg = 30;
                health = 60;
                speed = 10;
                attackRange = 35f;
                break;
            case "Heavy":
                Dmg = 60;
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

    private IEnumerator Wander()
    {
        // Define the area where the mob will wander
        float minX = transform.position.x - wanderRange;
        float maxX = transform.position.x + wanderRange;
        float minZ = transform.position.z - wanderRange;
        float maxZ = transform.position.z + wanderRange;

        while (true)
        {
            // Choose a random position within the wander range
            desiredPosition = new Vector3(
                Random.Range(minX, maxX),
                transform.position.y, // Keep the current Y position
                Random.Range(minZ, maxZ)
            );

            // Ensure the chosen position is on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(desiredPosition, out hit, wanderRange, NavMesh.AllAreas))
            {
                // Set the NavMeshAgent's destination
                navMeshAgent.SetDestination(hit.position);

                // Manually rotate the mob to face the new direction
                while (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
                {
                    // Get direction of movement
                    Vector3 direction = -navMeshAgent.velocity;

                    // If the velocity direction is not zero, rotate the mob
                    if (direction != Vector3.zero)
                    {
                        // Make the mob rotate smoothly towards the direction of movement
                        Quaternion toRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
                    }

                    yield return null;
                }
            }

            // Wait before choosing a new destination
            yield return new WaitForSeconds(2f);
        }
    }
}
