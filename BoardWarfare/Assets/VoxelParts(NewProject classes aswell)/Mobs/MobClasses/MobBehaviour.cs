using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MobBehaviour : MonoBehaviour
{
    private Vector3 wanderPosition;
    private float rotationSpeed = 5f;
    private float wanderRange = 15f;
    private float attackRange = 5f;
    private float detectionRange = 15f;
    private float stopChaseRange = 5f;
    private bool isAttacking = false;
    private bool isWalking = false;
    private bool isInToWalkingPlaying = false;

    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private Transform playerTransform;

    void Start()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.radius = 0.5f;
        navMeshAgent.avoidancePriority = Random.Range(0, 99);
        navMeshAgent.updateRotation = false;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        StartCoroutine(Wander());
    }

    void Update()
    {
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= attackRange)
            {
                if (!isAttacking)
                {
                    StartCoroutine(PerformAttack());
                }
            }
            else
            {
                if (isAttacking)
                {
                    StopAttack();
                }

                if (distanceToPlayer <= detectionRange)
                {
                    MoveToPlayer();
                }
                else if (!isWalking)
                {
                    
                    StartCoroutine(Wander());
                }
            }
        }

        RotateTowardsTarget();
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        navMeshAgent.isStopped = true;

        animator.SetBool("Walking", false);
        animator.SetBool("SeeEnemy", true);
        animator.SetTrigger("IsAttacking");

        while (isAttacking)
        {
            yield return new WaitForSeconds(1f);
        }
    }

    private void StopAttack()
    {
        isAttacking = false;
        animator.SetBool("SeeEnemy", false);
        navMeshAgent.isStopped = false;
    }

    private void MoveToPlayer()
    {
        if (!isWalking)
        {
            StartCoroutine(StartWalking(playerTransform.position));
        }
    }

    private IEnumerator StartWalking(Vector3 targetPosition)
    {
        isWalking = true;
        isInToWalkingPlaying = true;
        navMeshAgent.isStopped = true;

        animator.SetTrigger("InToWalking");
        yield return new WaitForSeconds(2f);

        animator.SetBool("Walking", true);
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(targetPosition);
        isInToWalkingPlaying = false;

        while (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            yield return null;
        }

        animator.SetBool("Walking", false);
        isWalking = false;
    }

    private IEnumerator Wander()
    {
        yield return new WaitForSeconds(5);
        isWalking = true;

        wanderPosition = new Vector3(
            Random.Range(transform.position.x - wanderRange, transform.position.x + wanderRange),
            transform.position.y,
            Random.Range(transform.position.z - wanderRange, transform.position.z + wanderRange)
        );

        if (NavMesh.SamplePosition(wanderPosition, out NavMeshHit hit, wanderRange, NavMesh.AllAreas))
        {
            yield return StartWalking(hit.position);
        }

        yield return new WaitForSeconds(3f);
        isWalking = false;
        yield return new WaitForSeconds(3f);
    }

    private void RotateTowardsTarget()
    {
        Vector3 targetDirection = Vector3.zero;

        if (isAttacking)
        {
            targetDirection = playerTransform.position - transform.position;
        }
        else if (isWalking)
        {
            targetDirection = navMeshAgent.destination - transform.position;
        }

        targetDirection.y = 0;

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
