using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "AI/States/WanderingState")]
public class WanderingState : StateBehaviour
{
    private NavMeshAgent agent;
    private Transform mobTransform;

    public float wanderMinRadius = 3f; // Минимальная дистанция
    public float wanderMaxRadius = 10f; // Максимальная дистанция
    public float waitTimeAtPoint = 2f; // Время ожидания
    public float rotationSpeed = 5f; // Скорость поворота

    private bool isWaiting = false;
    private float waitTimer = 0f;

    public override void Initialize(MobAI mobAI)
    {
        base.Initialize(mobAI);
        agent = mobAI.GetComponent<NavMeshAgent>();
        mobTransform = mobAI.transform;
    }

    public override void Enter()
    {
        Debug.Log("Entering Wandering State");
        MoveToNewPoint();
    }

    public override void Tick()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                isWaiting = false;
                MoveToNewPoint();
            }
        }
        else
        {
            // Проверяем, дошёл ли моб до точки
            if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
            {
                isWaiting = true;
                waitTimer = 0f;
            }
        }

        RotateAwayFrom(agent.destination);
    }

    public override void Exit()
    {
        Debug.Log("Exiting Wandering State");
        isWaiting = false;
    }

    private void MoveToNewPoint()
    {
        Vector3 randomPoint = GetValidNavMeshLocation();

        if (randomPoint != mobTransform.position) // Если точка не совпадает с текущей позицией
        {
            agent.SetDestination(randomPoint);
        }
        else
        {
            Debug.LogWarning("Не удалось найти подходящую точку. Пробуем снова...");
            MoveToNewPoint();
        }
    }

    private Vector3 GetValidNavMeshLocation()
    {
        int maxAttempts = 10;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere.normalized * wanderMaxRadius;
            Vector3 randomPoint = mobTransform.position + randomDirection;

            float distance = Vector3.Distance(mobTransform.position, randomPoint);

            if (distance < wanderMinRadius || distance > wanderMaxRadius)
                continue; // Пропускаем неподходящие точки

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return mobTransform.position; // Если точку не нашли, остаётся на месте
    }

    private void RotateAwayFrom(Vector3 target)
    {
        Vector3 directionToTarget = target - mobTransform.position;
        directionToTarget.y = 0f;

        Vector3 oppositeDirection = -directionToTarget;

        if (oppositeDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(oppositeDirection);
            mobTransform.rotation = Quaternion.Slerp(mobTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
