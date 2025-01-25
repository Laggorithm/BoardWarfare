using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MobBehaviour : MonoBehaviour
{
    private Queue<IEnumerator> actionQueue = new Queue<IEnumerator>();
    private bool isPerformingAction = false;
    private Vector3 desiredPosition;
    private int rotationSpeed = 150;
    private float wanderRange = 15f;
    private int speed;
    private float attackRange;
    private float Dmg;
    private string unitClass;
    private float health;
    private NavMeshAgent navMeshAgent;
    private Animator animator; // Ссылка на Animator

    void Start()
    {
        // Получаем компонент Animator
        animator = GetComponent<Animator>();

        // Initialize stats based on wave difficulty and unit class
        InitializeUnitStats();

        // Initialize the NavMeshAgent for movement
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.radius = 0.5f;
        navMeshAgent.avoidancePriority = Random.Range(0, 99);
        navMeshAgent.updateRotation = false;

        // Start the wandering behavior
        StartCoroutine(Wander());
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

    private IEnumerator Wander()
    {
        float minX = transform.position.x - wanderRange;
        float maxX = transform.position.x + wanderRange;
        float minZ = transform.position.z - wanderRange;
        float maxZ = transform.position.z + wanderRange;

        while (true)
        {
            // Включаем триггер для перехода к анимации ходьбы
            animator.SetTrigger("InToWalking");
            yield return new WaitForSeconds(2f); // Ждём 2 секунды (120 кадров, 60 FPS)

            // Активируем анимацию ходьбы
            animator.SetBool("Walking", true);

            // Выбираем новую точку для движения
            desiredPosition = new Vector3(
                Random.Range(minX, maxX),
                transform.position.y,
                Random.Range(minZ, maxZ)
            );

            // Проверяем, находится ли позиция на NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(desiredPosition, out hit, wanderRange, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);

                // Поворачиваем персонажа к цели
                while (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
                {
                    Vector3 direction = -navMeshAgent.velocity;

                    if (direction != Vector3.zero)
                    {
                        Quaternion toRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
                    }

                    yield return null;
                }
            }

            // Персонаж остановился, отключаем анимацию ходьбы
            animator.SetBool("Walking", false);

            // Ждём 7 секунд (420 кадров, 60 FPS) перед следующим циклом
            yield return new WaitForSeconds(7f);
        }
    }
}
