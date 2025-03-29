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

    public bool walkBackwards = false; // Флаг, чтобы двигаться или смотреть в обратном направлении
    public int rotationSpeed;
    public void SetPatrolPoints(Transform[] points)
    {
        patrolPoints = points;
    }

    public override void Initialize(MobAI mobAI)
    {
        base.Initialize(mobAI);
        agent = mobAI.GetComponent<NavMeshAgent>();

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("No patrol points assigned!");
        }
    }

    public override void Enter()
    {
        Debug.Log("Entering Patrol State");

        if (patrolPoints.Length == 0)
        {
            Debug.LogError("No patrol points to move to!");
            return;
        }

        // Устанавливаем начальную точку
        currentPatrolIndex = walkBackwards ? patrolPoints.Length - 1 : 0;
        MoveToNextPoint();
    }

    public override void Tick()
    {
        if (patrolPoints.Length == 0)
        {
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

        // Поворот в направлении цели или в обратном направлении в зависимости от флага
        RotateTowardsTarget(agent.destination);
    }

    public override void Exit()
    {
        Debug.Log("Exiting Patrol State");
    }

    private void MoveToNextPoint()
    {
        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning("No patrol points available!");
            return;
        }

        // Если двигаемся вперед, идем к следующей точке, если назад - к предыдущей
        Transform targetPoint = patrolPoints[currentPatrolIndex];

        // Устанавливаем целевую точку
        agent.SetDestination(targetPoint.position);

        // Обновляем индекс патрульной точки в зависимости от флага
        if (walkBackwards)
        {
            currentPatrolIndex--;
            if (currentPatrolIndex < 0)
            {
                currentPatrolIndex = patrolPoints.Length - 1; // Когда дошли до первой, идем в обратном порядке
            }
        }
        else
        {
            currentPatrolIndex++;
            if (currentPatrolIndex >= patrolPoints.Length)
            {
                currentPatrolIndex = 0; // Если дошли до последней, идем в начале
            }
        }
    }

    private void RotateTowardsTarget(Vector3 target)
    {
        // Вычисляем направление к цели
        Vector3 directionToTarget = target - agent.transform.position;
        directionToTarget.y = 0f;

        // Если флаг включен, разворачиваемся в противоположную сторону
        Vector3 finalDirection = walkBackwards ? -directionToTarget : directionToTarget;

        if (finalDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(finalDirection);

            // Плавный поворот, корректируем скорость
            agent.transform.rotation = Quaternion.RotateTowards(agent.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

}
