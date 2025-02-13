using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MobBehaviour : MonoBehaviour
{
    // Режимы работы моба
    private enum State
    {
        Patrol,   // Патруль
        Follow,   // Следование за игроком
        Attack    // Атака
    }

    [Header("Настройки модели")]
    [Tooltip("Если установлено, модель моба будет повернута на 180 градусов, чтобы он шёл спиной.")]
    public bool flipModel = false;

    [Header("Настройки патруля")]
    [Tooltip("Массив точек патруля, между которыми будет перемещаться моб. Если не назначены, будет выбран случайный патрульный маршрут.")]
    public Transform[] patrolPoints;
    [Tooltip("Время ожидания в точке патруля (в секундах)")]
    public float waitTimeAtPatrolPoint;

    [Header("Настройки преследования")]
    [Tooltip("Расстояние, на котором моб начинает замечать игрока")]
    public float seeEnemyDistance = 15f;
    [Tooltip("Расстояние до игрока, при котором моб начинает атаку")]
    public float attackRange;

    [Header("Ссылки на объекты")]
    [Tooltip("Ссылка на игрока")]
    public Transform player;

    // Компоненты
    private NavMeshAgent agent;
    private Animator anim;

    // Текущий режим и индекс патрульной точки
    private State currentState = State.Patrol;
    private int currentPatrolIndex = 0;

    // Флаги состояний
    private bool isWaiting = false;
    private bool isAttacking = false;
    private bool isDead = false; // Если true — моб мёртв

    [Header("Настройки поворота")]
    [Tooltip("Скорость поворота моба.")]
    public float rotationSpeed = 5f;

    // Здоровье моба
    private float health = 10f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // Если ссылка на игрока не задана, ищем его по тегу "Player"
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("Игрок с тегом 'Player' не найден!");
            }
        }

        // Если заданы патрульные точки, устанавливаем первую цель, иначе выбираем случайную точку
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        else
        {
            SetDestination(GetRandomPatrolPoint());
        }

        // Отключаем автоматическое обновление поворота агента
        agent.updateRotation = false;
    }

    void Update()
    {
        // Если моб мёртв — прекращаем всю логику
        if (isDead)
            return;

        // Отслеживание игрока только по горизонтали:
        Vector3 horizontalPlayerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        float distanceToPlayer = Vector3.Distance(transform.position, horizontalPlayerPos);

        // Если моб патрулирует и замечает игрока — переключаемся в режим преследования
        if (currentState == State.Patrol && distanceToPlayer <= seeEnemyDistance)
        {
            currentState = State.Follow;
            anim.SetBool("SeeEnemy", true);
            if (isWaiting)
            {
                StopCoroutine(PatrolWait());
                isWaiting = false;
                agent.isStopped = false;
            }
        }
        // Если моб преследует, а игрок уже ушёл — возвращаемся к патрулю
        else if (currentState == State.Follow && distanceToPlayer > seeEnemyDistance)
        {
            currentState = State.Patrol;
            anim.SetBool("SeeEnemy", false);
            if (patrolPoints != null && patrolPoints.Length > 0)
                SetDestination(patrolPoints[currentPatrolIndex].position);
            else
                SetDestination(GetRandomPatrolPoint());
        }

        // Обработка логики в зависимости от текущего режима
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Follow:
                Follow();
                break;
            case State.Attack:
                // Логика атаки реализована в корутине Attack()
                break;
        }
    }

    /// <summary>
    /// Логика патруля
    /// </summary>
    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isWaiting)
        {
            StartCoroutine(PatrolWait());
        }
        else
        {
            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                anim.SetBool("Walking", true);
                anim.SetBool("Idle", false);
            }
        }
    }

    /// <summary>
    /// Корутина ожидания в точке патруля
    /// </summary>
    IEnumerator PatrolWait()
    {
        isWaiting = true;
        agent.isStopped = true;
        anim.SetBool("Idle", true);
        anim.SetBool("Walking", false);

        yield return new WaitForSeconds(waitTimeAtPatrolPoint);

        agent.isStopped = false;
        isWaiting = false;

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        else
        {
            SetDestination(GetRandomPatrolPoint());
        }
    }

    /// <summary>
    /// Генерация случайной точки для патруля
    /// </summary>
    Vector3 GetRandomPatrolPoint()
    {
        float randomDistance = Random.Range(5f, 15f);
        Vector3 randomDirection = Random.insideUnitSphere * randomDistance;
        randomDirection.y = 0;
        return transform.position + randomDirection;
    }

    /// <summary>
    /// Логика преследования игрока
    /// </summary>
    void Follow()
    {
        if (isAttacking)
            return;

        // Отслеживание цели только по горизонтали:
        Vector3 horizontalPlayerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        SetDestination(horizontalPlayerPos);

        float distanceToPlayer = Vector3.Distance(transform.position, horizontalPlayerPos);
        if (distanceToPlayer <= attackRange)
        {
            StartCoroutine(Attack());
        }
        else
        {
            anim.SetBool("Walking", true);
            anim.SetBool("Idle", false);
        }
    }

    /// <summary>
    /// Корутина атаки
    /// </summary>
    IEnumerator Attack()
    {
        isAttacking = true;
        currentState = State.Attack;
        agent.isStopped = true;
        anim.SetTrigger("IsAttacking");

        yield return new WaitForSeconds(3f);

        agent.isStopped = false;
        isAttacking = false;
        currentState = State.Follow;
    }

    /// <summary>
    /// Устанавливает новую цель для NavMeshAgent и поворачивает моба в её сторону по горизонтали.
    /// </summary>
    void SetDestination(Vector3 destination)
    {
        // Отслеживание только по горизонтали: задаем Y равным Y моба
        destination.y = transform.position.y;
        Vector3 direction = destination - transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = flipModel ? Quaternion.LookRotation(-direction) : Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

        agent.SetDestination(destination);
    }

    /// <summary>
    /// Обработка столкновений (например, получение урона от пули)
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Если моб уже мёртв, не обрабатываем столкновения
        if (isDead)
            return;

        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                health -= bullet.damage;
                Debug.Log("Моб получил " + bullet.damage + " урона, оставшееся здоровье: " + health);

                // Если здоровье меньше или равно 0, активируем режим смерти
                if (health <= 0)
                {
                    isDead = true;
                    agent.isStopped = true;  // Останавливаем движение сразу
                    anim.SetTrigger("Die");  // Запускаем анимацию смерти (убедитесь, что в Animator есть триггер "Die")
                    StartCoroutine(Die());
                }
            }
        }
    }

    /// <summary>
    /// Корутина смерти: после активации анимации смерти ждём 4 секунды и удаляем объект.
    /// Пока isDead == true, все остальные действия прекращаются.
    /// </summary>
    IEnumerator Die()
    {
        // Можно дополнительно отключить коллайдер, чтобы не было лишних столкновений
        GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(4f);
        Destroy(gameObject);
    }
}
