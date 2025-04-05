using UnityEngine;
using UnityEngine.AI;

public class RangedAttackState : StateBehaviour
{
    public GameObject projectilePrefab;
    public Transform shootingPoint;
    public int waitFrames; // Количество фреймов ожидания после выстрела

    private NavMeshAgent agent;
    private Animator anim;
    private Transform player;
    private MobAI mobAI;
    private int currentWaitFrames; // Текущее количество фреймов ожидания

    public override void Initialize(MobAI mobAI)
    {
        base.Initialize(mobAI);
        this.mobAI = mobAI;
        agent = mobAI.GetComponent<NavMeshAgent>();
        anim = mobAI.GetComponent<Animator>();
        player = mobAI.player;
    }

    public override void Enter()
    {
        Debug.Log("RangedAttack");
        agent.isStopped = true; // Останавливаем движение
        agent.updateRotation = false; // Отключаем автоматический поворот

        anim.SetTrigger("SecondPhaseAttack"); // Запускаем анимацию атаки
        Shoot(); // Стреляем
        currentWaitFrames = waitFrames; // Инициализируем количество фреймов ожидания
    }

    public override void Tick()
    {
        if (currentWaitFrames > 0)
        {
            // Ожидаем нужное количество фреймов
            currentWaitFrames--;
        }
    }

    public override void Exit()
    {
        agent.isStopped = false; // Включаем движение
        agent.updateRotation = true; // Включаем автоматический поворот
        Debug.Log("Exiting RangedAttack State");
    }

    private void Shoot()
    {
        if (projectilePrefab != null && shootingPoint != null)
        {
            GameObject projectile = GameObject.Instantiate(projectilePrefab, shootingPoint.position, shootingPoint.rotation);
            Debug.Log("Ranged attack: Projectile fired!");
        }
    }
}
