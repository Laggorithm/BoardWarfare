using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "AI/States/PatrolState")]
public class PatrolState : StateBehaviour
{
    private NavMeshAgent agent;
    private int currentPatrolIndex;
    private float waitTimer;

    public Transform[] patrolPoints;
    public float waitTime = 2f;

    public override void Initialize(MobAI mobAI)
    {
        base.Initialize(mobAI);
        agent = mobAI.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        Debug.Log("Entering Patrol State");
        MoveToNextPoint();
    }

    public override void Tick()
    {
        if (patrolPoints.Length == 0)
        {
            // Здесь ничего не происходит, если нет патрульных точек
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                MoveToNextPoint();
                waitTimer = 0f;
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Patrol State");
    }

    private void MoveToNextPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }
}
