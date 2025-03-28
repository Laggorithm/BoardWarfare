using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "AI/States/WanderingState")]
public class WanderingState : StateBehaviour
{
    private NavMeshAgent agent;
    private Transform mobTransform;

    public float wanderRadius = 10f;
    public float wanderTimer = 5f; // Время, через которое моб будет двигаться в новую точку
    private float timeSinceLastWander = 0f;

    public float rotationSpeed = 5f; // Скорость поворота

    public override void Initialize(MobAI mobAI)
    {
        base.Initialize(mobAI);
        agent = mobAI.GetComponent<NavMeshAgent>();
        mobTransform = mobAI.transform;
    }

    public override void Enter()
    {
        Debug.Log("Entering Wandering State");
        timeSinceLastWander = wanderTimer; // Чтобы сразу начать движение
    }

    public override void Tick()
    {
        timeSinceLastWander += Time.deltaTime;

        if (timeSinceLastWander >= wanderTimer)
        {
            Vector3 randomPoint = GetRandomNavMeshLocation();
            agent.SetDestination(randomPoint);
            timeSinceLastWander = 0f; // Сбросить таймер
        }

        // Поворот моба в противоположную сторону от цели
        RotateAwayFrom(agent.destination);
    }

    public override void Exit()
    {
        Debug.Log("Exiting Wandering State");
    }

    private Vector3 GetRandomNavMeshLocation()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += agent.transform.position;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas);
        return hit.position;
    }

    private void RotateAwayFrom(Vector3 target)
    {
        Vector3 directionToTarget = target - mobTransform.position;
        directionToTarget.y = 0f;  // Оставляем только горизонтальное направление

        // Инвертируем направление, чтобы моб смотрел в противоположную сторону
        Vector3 oppositeDirection = -directionToTarget;

        if (oppositeDirection.sqrMagnitude > 0.01f) // Поворот только если есть достаточно расстояния
        {
            Quaternion targetRotation = Quaternion.LookRotation(oppositeDirection);
            mobTransform.rotation = Quaternion.Slerp(mobTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
