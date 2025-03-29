using UnityEngine;
using UnityEngine.AI;

public class AttackState : StateBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private Transform player;
    private float attackCooldown = 1.5f;
    private float lastAttackTime;
    public float rotationSpeed = 5f; // Скорость поворота

    public override void Initialize(MobAI mobAI)
    {
        base.Initialize(mobAI);
        agent = mobAI.GetComponent<NavMeshAgent>();
        anim = mobAI.GetComponent<Animator>();
        player = mobAI.player;
    }

    public override void Enter()
    {
        agent.isStopped = true;
        agent.updateRotation = false; // Отключаем автоматический поворот агента
        anim.SetTrigger("Attack");
        RotateAwayFrom(player.position); // Разворачиваем моба спиной к игроку
    }

    public override void Tick()
    {
        if (Time.time - lastAttackTime > attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    public override void Exit()
    {
        agent.isStopped = false;
        agent.updateRotation = true; // Включаем обратно после атаки
    }

    private void Attack()
    {
        Debug.Log("Mob attacks!");
        RotateAwayFrom(player.position); // Повторный поворот спиной во время атаки
    }

    // Поворот спиной к игроку
    private void RotateAwayFrom(Vector3 target)
    {
        Vector3 directionToTarget = target - agent.transform.position;
        directionToTarget.y = 0f;  // Оставляем только горизонтальное направление

        Vector3 oppositeDirection = -directionToTarget; // Инвертируем направление

        if (oppositeDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(oppositeDirection);
            agent.transform.rotation = targetRotation; // Принудительный разворот
        }
    }
}