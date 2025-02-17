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

    [Header("Настройки моба")]
    [Tooltip("Если установлено, модель моба будет повернута на 180 градусов, чтобы он шёл спиной.")]
    public bool flipModel = false;
    public float health = 10f;

    [Header("Настройки патруля")]
    [Tooltip("Массив точек патруля, между которыми будет перемещаться моб. Если не назначены, будет выбран случайный патрульный маршрут.")]
    public Transform[] patrolPoints;
    [Tooltip("Время ожидания в точке патруля (в секундах)")]
    public float waitTimeAtPatrolPoint = 2f;

    [Header("Настройки преследования")]
    [Tooltip("Расстояние, на котором моб начинает замечать игрока")]
    public float seeEnemyDistance = 15f;
    [Tooltip("Расстояние до игрока, при котором моб начинает атаку")]
    public float attackRange = 2f;

    [Header("Ссылки на объекты")]
    [Tooltip("Ссылка на игрока")]
    public Transform player;

    [Header("Настройки поворота")]
    [Tooltip("Скорость поворота моба.")]
    public float rotationSpeed = 5f;

    // Компоненты
    private NavMeshAgent agent;
    private Animator anim;

    // Текущий режим и индекс патрульной точки
    private State currentState = State.Patrol;
    private int currentPatrolIndex = 0;

    // Флаги состояний
    private bool isWaiting = false;
    private bool isAttacking = false;
    private bool isDead = false; // Моб мёртв
    private bool deathStarted = false; // Гарантирует, что смерть обрабатывается только один раз

    // Управление корутинами
    private Coroutine patrolWaitCoroutine;
    private Coroutine attackCoroutine;
    private Coroutine hitTurnCoroutine;

    // Параметры для реакции на удары
    private float lastHitTime = 0f;
    private bool isTakingHits = false;
    private float turnCooldown = 0.3f; // Период между поворотами
    private float hitDuration = 1f;    // Длительность эффекта удара
    private float lastTurnTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // Если ссылка на игрока не задана, ищем его по тегу "Player"
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning("Игрок с тегом 'Player' не найден!");
        }

        // Устанавливаем первую точку назначения
        if (patrolPoints != null && patrolPoints.Length > 0)
            SetDestination(patrolPoints[currentPatrolIndex].position);
        else
            SetDestination(GetRandomPatrolPoint());

        // Отключаем автоматический поворот агента (поворот будем задавать вручную)
        agent.updateRotation = false;
    }

    void Update()
    {
        // Если здоровье меньше или равно 0 и смерть ещё не начата, запускаем смерть один раз
        if (health <= 0 && !deathStarted)
        {
            deathStarted = true;
            isDead = true;
            agent.isStopped = true;
            anim.SetTrigger("Die"); // Убедитесь, что в клипе смерти Loop Time отключён
            StartCoroutine(Die());
        }
        if (isDead)
            return;

        // Если моб получает удары, обновляем таймер; если прошло больше hitDuration, сбрасываем режим ударов
        if (isTakingHits && Time.time - lastHitTime > hitDuration)
        {
            isTakingHits = false;
            anim.SetBool("IsHit", false);
        }

        // Расстояние до игрока (рассчитываем только по горизонтали)
        Vector3 horizontalPlayerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        float distanceToPlayer = Vector3.Distance(transform.position, horizontalPlayerPos);

        // Переходы между режимами
        if (currentState == State.Patrol && distanceToPlayer <= seeEnemyDistance)
        {
            currentState = State.Follow;
            anim.SetBool("SeeEnemy", true);
            if (isWaiting && patrolWaitCoroutine != null)
            {
                StopCoroutine(patrolWaitCoroutine);
                patrolWaitCoroutine = null;
                isWaiting = false;
                agent.isStopped = false;
            }
        }
        else if (currentState == State.Follow && distanceToPlayer > seeEnemyDistance)
        {
            currentState = State.Patrol;
            anim.SetBool("SeeEnemy", false);
            // Возобновляем патруль, выбирая следующую точку
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

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isWaiting)
        {
            if (patrolWaitCoroutine != null)
            {
                StopCoroutine(patrolWaitCoroutine);
            }
            patrolWaitCoroutine = StartCoroutine(PatrolWait());
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
        patrolWaitCoroutine = null;
    }

    Vector3 GetRandomPatrolPoint()
    {
        float randomDistance = Random.Range(5f, 15f);
        Vector3 randomDirection = Random.insideUnitSphere * randomDistance;
        randomDirection.y = 0;
        return transform.position + randomDirection;
    }

    void Follow()
    {
        if (isAttacking)
            return;

        Vector3 horizontalPlayerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        SetDestination(horizontalPlayerPos);

        float distanceToPlayer = Vector3.Distance(transform.position, horizontalPlayerPos);
        if (distanceToPlayer <= attackRange)
        {
            if (!isAttacking)
            {
                // Начинаем атаку
                attackCoroutine = StartCoroutine(Attack());
            }
        }
        else
        {
            anim.SetBool("Walking", true);
            anim.SetBool("Idle", false);
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        currentState = State.Attack;
        agent.isStopped = true;
        anim.SetTrigger("IsAttacking");

        // Здесь можно добавить дополнительные проверки (например, находится ли игрок всё ещё в зоне атаки)
        yield return new WaitForSeconds(3f);

        if (!isDead)
        {
            agent.isStopped = false;
            isAttacking = false;
            currentState = State.Follow;
        }
        attackCoroutine = null;
    }

    // Устанавливает цель для агента и поворачивает моба, если он не получает ударов
    void SetDestination(Vector3 destination)
    {
        destination.y = transform.position.y; // Фиксируем Y координату

        if (!isTakingHits)
        {
            Vector3 direction = destination - transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = flipModel ? Quaternion.LookRotation(-direction) : Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
        agent.SetDestination(destination);
    }

    // Замораживает движение агента на указанное время
    IEnumerator FreezeAgent(float duration)
    {
        bool previousStopped = agent.isStopped;
        agent.isStopped = true;
        yield return new WaitForSeconds(duration);
        if (!isTakingHits && !isDead)
            agent.isStopped = false;
        else
            agent.isStopped = previousStopped;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead)
            return;

        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                health -= bullet.damage;
                Debug.Log("Моб получил " + bullet.damage + " урона, оставшееся здоровье: " + health);

                // Уничтожаем пулю после столкновения
                Destroy(other.gameObject);

                if (health > 0)
                {
                    Vector3 hitDirection = (transform.position - bullet.transform.position).normalized;
                    TakeHit(hitDirection);
                }
            }
        }
    }

    // Реагирует на удар: запускает цепочку анимаций и поворот в сторону игрока
    public void TakeHit(Vector3 hitDirection)
    {
        lastHitTime = Time.time;
        isTakingHits = true;
        anim.SetTrigger("HitTrigger");
        anim.SetBool("IsHit", true);

        // Замораживаем движение агента на 1 секунду
        StartCoroutine(FreezeAgent(1f));

        // Поворачиваем моба в сторону игрока
        Vector3 directionToPlayer = new Vector3(player.position.x - transform.position.x, 0, player.position.z - transform.position.z).normalized;
        if (Time.time - lastTurnTime > turnCooldown)
        {
            lastTurnTime = Time.time;
            if (hitTurnCoroutine != null)
            {
                StopCoroutine(hitTurnCoroutine);
            }
            hitTurnCoroutine = StartCoroutine(TurnTowardsHit(directionToPlayer));
        }
    }

    // Плавно поворачивает моба в сторону targetDir
    IEnumerator TurnTowardsHit(Vector3 targetDir)
    {
        yield return new WaitForSeconds(turnCooldown);

        if (flipModel)
        {
            targetDir = -targetDir;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetDir);
        float t = 0f;
        float duration = 0.3f;
        Quaternion initialRotation = transform.rotation;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t / duration);
            yield return null;
        }
        hitTurnCoroutine = null;
    }

    IEnumerator Die()
    {
        GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(4f);
        anim.enabled = false;
        Destroy(gameObject);
    }
}
