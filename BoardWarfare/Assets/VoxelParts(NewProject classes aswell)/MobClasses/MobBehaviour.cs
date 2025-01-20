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

        // Start the wandering behavior
        StartCoroutine(CheckConditions());
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

    private IEnumerator CheckConditions()
    {
        while (true)
        {
            // If no actions are pending, enqueue wandering
            if (actionQueue.Count == 0 && !isPerformingAction)
            {
                EnqueueAction(Wander);
            }

            yield return new WaitForSeconds(2f); // Re-evaluate conditions periodically
        }
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

    private IEnumerator Wander()
    {
        Animator animator = GetComponent<Animator>();

        // Define the map boundaries
        float minX = 145f, maxX = 180f;
        float minZ = -83f, maxZ = 0f;

        // Calculate a random wander position
        desiredPosition = new Vector3(
            Random.Range(minX, maxX),
            transform.position.y, // Keep the current Y position
            Random.Range(minZ, maxZ)
        );

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
}
