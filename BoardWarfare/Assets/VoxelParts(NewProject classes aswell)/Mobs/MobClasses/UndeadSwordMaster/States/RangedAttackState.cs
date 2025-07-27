using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RangedAttackState : StateBehaviour
{
    public GameObject projectilePrefab;
    public Transform shootingPoint;
    public int waitFrames; // Количество фреймов ожидания перед выстрелом

    private NavMeshAgent agent;
    private Animator anim;
    private Transform player;
    private MobAI mobAI;
    private int currentWaitFrames; // Текущее количество фреймов ожидания

    // Этот флаг будет использоваться для проверки, можно ли выходить из состояния
    public override bool CanExit => currentWaitFrames <= 0;

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
        currentWaitFrames = waitFrames;
        Debug.Log("RangedAttack");
        agent.isStopped = true; // Останавливаем движение
        agent.updateRotation = false; // Отключаем автоматический поворот

        anim.SetTrigger("SecondPhaseAttack"); // Запускаем анимацию атаки
        Shoot(); // Стреляем
    }

    public override void Tick()
    {
        // Делаем проверку фреймов здесь, а не в CanExit
        if (currentWaitFrames > 0)
        {
            // Ожидаем нужное количество фреймов
            currentWaitFrames--;

            Debug.Log(currentWaitFrames);
        }
        else
        {
            // После того как фреймы закончились, вызываем выход из состояния
            // Этот код сработает только после окончания отсчёта
            Exit();
        }
    }

    private void Shoot()
    {
        if (projectilePrefab != null && shootingPoint != null)
        {
            // Создаём снаряд в точке выстрела и с нужной ориентацией
            GameObject projectile = Instantiate(projectilePrefab, shootingPoint.position, shootingPoint.rotation);
            Debug.Log("Ranged attack: Projectile fired!");

            // Получаем компонент Rigidbody снаряда
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Направление выстрела (вперёд по оси Z)
                Vector3 forwardDirection = shootingPoint.forward;

                // Устанавливаем скорость снаряда
                float projectileSpeed = 30f; // Скорость снаряда
                rb.linearVelocity = forwardDirection * projectileSpeed;

                // Запускаем корутину для удаления снаряда через 2 секунды
                mobAI.StartCoroutine(DestroyProjectileAfterDelay(projectile, 2f)); // Удалить через 2 секунды
            }
            else
            {
                Debug.LogError("Rigidbody не найден на снаряде!");
            }
        }
        else
        {
            Debug.LogError("Prefab снаряда или ShootingPoint не назначены!");
        }
    }

    private IEnumerator DestroyProjectileAfterDelay(GameObject projectile, float delay)
    {
        // Ожидаем указанное время
        yield return new WaitForSeconds(delay);

        // Удаляем снаряд
        Destroy(projectile);
    }

    public override void Exit()
    {
        if (agent != null)
        {
            agent.isStopped = false; // Включаем движение
            agent.updateRotation = true; // Включаем автоматический поворот
        }

        Debug.Log("Exiting RangedAttack State");
    }
}
