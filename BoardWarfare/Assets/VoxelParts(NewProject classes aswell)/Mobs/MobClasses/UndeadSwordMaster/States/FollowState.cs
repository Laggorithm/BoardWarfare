using UnityEngine;
using UnityEngine.AI;

public class FollowState : StateBehaviour
{
    private NavMeshAgent agent;
    public Transform player;
    public float stopDistance = 2f; // Расстояние, при котором моб начинает атаковать

    public float rotationSpeed = 5f; // Скорость поворота

    public bool walkBackwards = false; // Флаг для движения задом

    public override void Initialize(MobAI mobAI)
    {
        base.Initialize(mobAI);
        agent = mobAI.GetComponent<NavMeshAgent>();
        agent.updateRotation = true; // Включаем автоматическое вращение
    }

    public override void Enter()
    {
        Debug.Log("Entering Follow State");

        // Попробуем найти игрока по тегу
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform; // Присваиваем игрока
            }
        }

        // Сбросим путь перед входом в состояние
        if (agent.isOnNavMesh)
        {
            agent.ResetPath();  // Сбросим текущий путь
        }
    }

    public override void Tick()
    {
        if (player != null)
        {
            // Обновляем цель каждый тик
            agent.SetDestination(player.position); // Двигаемся к игроку

            // Поворот моба в противоположную сторону от игрока (если нужно)
            if (walkBackwards)
            {
                RotateAwayFrom(player.position); // Разворачиваем спиной, если флаг установлен
            }
            else
            {
                RotateTowards(player.position); // Поворот лицом к игроку
            }

            // Проверяем расстояние до игрока
            float distanceToPlayer = Vector3.Distance(mob.transform.position, player.position);

            if (distanceToPlayer <= stopDistance) // Когда до игрока осталось меньше расстояния для атаки
            {
                mob.GetComponent<StateMachine>().SetState(mob.GetComponent<AttackState>());
            }
        }
        else
        {
            Debug.LogWarning("Player не найден в FollowState!");
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Follow State");
        // Убираем любые дополнительные действия при выходе, если нужно
    }

    // Поворот в противоположную сторону от игрока
    private void RotateAwayFrom(Vector3 target)
    {
        Vector3 directionToTarget = target - agent.transform.position;
        directionToTarget.y = 0f;  // Оставляем только горизонтальное направление

        // Инвертируем направление, чтобы моб смотрел в противоположную сторону от игрока
        Vector3 oppositeDirection = -directionToTarget;

        if (oppositeDirection.sqrMagnitude > 0.01f) // Поворот только если есть достаточно расстояния
        {
            Quaternion targetRotation = Quaternion.LookRotation(oppositeDirection);
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Поворот в сторону игрока
    private void RotateTowards(Vector3 target)
    {
        Vector3 directionToTarget = target - agent.transform.position;
        directionToTarget.y = 0f;  // Оставляем только горизонтальное направление

        if (directionToTarget.sqrMagnitude > 0.01f) // Поворот только если есть достаточно расстояния
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
