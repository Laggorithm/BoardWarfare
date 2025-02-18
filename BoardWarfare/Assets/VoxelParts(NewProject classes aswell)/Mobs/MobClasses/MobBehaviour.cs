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
        Attack,   // Атака
        Ultimate  // Использование ультимейта
    }

    [Header("Настройки моба")]
    [Tooltip("Если установлено, модель моба будет повернута на 180 градусов, чтобы он шёл спиной.")]
    public bool flipModel = false;
    public float health = 10f;

    [Header("Настройки патруля")]
    [Tooltip("Массив точек патруля, между которыми будет перемещаться моб. Если не назначены, выбирается случайная точка.")]
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
    [Tooltip("Скорость поворота моба")]
    public float rotationSpeed = 5f;

    // --- Параметры для реакции на удары ---
    [Tooltip("Период между поворотами при получении удара")]
    public float turnCooldown = 0.3f;
    [Tooltip("Длительность эффекта удара (если между ударами прошло больше этого времени, сбрасывается состояние удара)")]
    public float hitDuration = 1f;

    // --- Настройки ускорения (Follow) ---
    [Tooltip("Исходная скорость агента (будет сохранена автоматически)")]
    public float normalSpeed; // сохраняется в Start()
    private Coroutine boostRoutine;  // ссылка на корутину проверки ускорения

    // --- Настройки ультимейта ---
    [Tooltip("Минимальное время между использованием ультимейта (в секундах)")]
    public float ultimateCooldown = 10f;
    [Tooltip("Время последнего ультимейта")]
    public float lastUltimateTime = -10f;
    private Coroutine ultimateChanceCoroutine;  // корутина проверки шанса ультимейта

    // --- Тестовые настройки (для отладки) ---
    [Header("Тестовые настройки")]
    [Tooltip("Если включено, при старте моб будет оглушён (для теста)")]
    public bool testStunOnStart = false;
    [Tooltip("Длительность тестового эффекта стана (в секундах)")]
    public float testStunDuration = 10f;

    // --- Внутренние переменные состояния ---
    private State currentState = State.Patrol;
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private bool isAttacking = false;
    private bool isDead = false;      // моб мёртв
    private bool deathStarted = false; // смерть обрабатывается один раз
    private float lastHitTime = 0f;
    private bool isTakingHits = false;
    private float lastTurnTime = 0f;

    // Компоненты
    private NavMeshAgent agent;
    private Animator anim;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // Сохраняем исходную скорость агента
        normalSpeed = agent.speed;

        // Если ссылка на игрока не задана, ищем его по тегу "Player"
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning("Игрок с тегом 'Player' не найден!");
        }

        // Устанавливаем цель: либо первая патрульная точка, либо случайная
        if (patrolPoints != null && patrolPoints.Length > 0)
            SetDestination(patrolPoints[currentPatrolIndex].position);
        else
            SetDestination(GetRandomPatrolPoint());

        // Отключаем автоматическое обновление поворота агента
        agent.updateRotation = false;

        // Тестовый эффект стана (для отладки)
        if (testStunOnStart)
        {
            StartCoroutine(TestStunRoutine());
        }
    }

    void Update()
    {
        // Если моб мёртв – дальше ничего не делаем
        if (health <= 0 && !deathStarted)
        {
            deathStarted = true;
            isDead = true;
            agent.isStopped = true;
            anim.SetTrigger("Die");
            StartCoroutine(Die());
        }
        if (isDead)
            return;

        // Если моб находится в состоянии Ultimate, не выполняем другие действия
        if (currentState == State.Ultimate)
            return;

        // Обновляем состояние получения удара
        if (isTakingHits && Time.time - lastHitTime > hitDuration)
        {
            isTakingHits = false;
            anim.SetBool("IsHit", false);
        }

        // Отслеживаем позицию игрока (по горизонтали)
        Vector3 horizontalPlayerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        float distanceToPlayer = Vector3.Distance(transform.position, horizontalPlayerPos);

        // Переключаем стейты по расстоянию
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
        else if (currentState == State.Follow && distanceToPlayer > seeEnemyDistance)
        {
            currentState = State.Patrol;
            anim.SetBool("SeeEnemy", false);
            if (patrolPoints != null && patrolPoints.Length > 0)
                SetDestination(patrolPoints[currentPatrolIndex].position);
            else
                SetDestination(GetRandomPatrolPoint());
        }

        // Выполнение логики по состояниям
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Follow:
                Follow();
                break;
            case State.Attack:
                Attack();
                break;
            case State.Ultimate:
                UltimateRoutine();
                break;
        }

        // Управление ускорением в режиме Follow
        if (currentState == State.Follow)
        {
            if (boostRoutine == null)
                boostRoutine = StartCoroutine(SpeedBoostCheck());
        }
        else
        {
            if (boostRoutine != null)
            {
                StopCoroutine(boostRoutine);
                boostRoutine = null;
                agent.speed = normalSpeed;
                anim.speed = 1f;
            }
        }

        // Проверка шанса ультимейта: только в режиме Follow, когда кулдаун истёк
        if (currentState == State.Follow && (Time.time - lastUltimateTime >= ultimateCooldown) && ultimateChanceCoroutine == null)
        {
            ultimateChanceCoroutine = StartCoroutine(UltimateChanceCheck());
        }
        else if (currentState != State.Follow && ultimateChanceCoroutine != null)
        {
            StopCoroutine(ultimateChanceCoroutine);
            ultimateChanceCoroutine = null;
        }
    }

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isWaiting)
        {
            StartCoroutine(PatrolWait());
        }
        else if (agent.velocity.sqrMagnitude > 0.1f)
        {
            anim.SetBool("Walking", true);
            anim.SetBool("Idle", false);
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
            StartCoroutine(Attack());
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

        yield return new WaitForSeconds(3f);

        agent.isStopped = false;
        isAttacking = false;
        currentState = State.Follow;
    }

    // SetDestination с обновлением поворота (если не получает удары)
    void SetDestination(Vector3 destination)
    {
        destination.y = transform.position.y;
        if (!isTakingHits)
        {
            Vector3 direction = destination - transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = flipModel ? Quaternion.LookRotation(-direction) : Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }
        }
        agent.SetDestination(destination);
    }

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

                if (health <= 0)
                {
                    // Смерть обрабатывается в Update()
                }
                else
                {
                    Vector3 hitDirection = (transform.position - bullet.transform.position).normalized;
                    TakeHit(hitDirection);
                }
            }
        }
    }

    public void TakeHit(Vector3 hitDirection)
    {
        lastHitTime = Time.time;
        isTakingHits = true;
        anim.SetTrigger("HitTrigger");
        anim.SetBool("IsHit", true);

        StartCoroutine(FreezeAgent(1f));

        Vector3 directionToPlayer = new Vector3(player.position.x - transform.position.x, 0, player.position.z - transform.position.z).normalized;
        if (Time.time - lastTurnTime > turnCooldown)
        {
            lastTurnTime = Time.time;
            StartCoroutine(TurnTowardsHit(directionToPlayer));
        }
    }

    IEnumerator TurnTowardsHit(Vector3 targetDir)
    {
        yield return new WaitForSeconds(turnCooldown);
        if (flipModel)
            targetDir = -targetDir;
        Quaternion targetRotation = Quaternion.LookRotation(targetDir);
        float t = 0f;
        float duration = 0.3f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t / duration);
            yield return null;
        }
    }

    IEnumerator Die()
    {
        GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(4f);
        anim.enabled = false;
        Destroy(gameObject);
    }

    IEnumerator SpeedBoostCheck()
    {
        while (true)
        {
            int roll = Random.Range(1, 4);
            if (roll == 3)
            {
                agent.speed = normalSpeed * 2;
                anim.speed = 2f;
            }
            else
            {
                agent.speed = normalSpeed;
                anim.speed = 1f;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator UltimateChanceCheck()
    {
        yield return new WaitForSeconds(2f);
        if (currentState == State.Follow && (Time.time - lastUltimateTime >= ultimateCooldown))
        {
            if (Random.value <= 0.7f)
            {
                currentState = State.Ultimate;
                agent.isStopped = true;
                yield return StartCoroutine(UltimateRoutine());
            }
        }
        ultimateChanceCoroutine = null;
    }

    // UltimateRoutine с общей длительностью 540 кадров:
    // 300 кадров (примерно 5 секунд) ожидания, затем 240 кадров (примерно 4 секунды) активации ульты.
    IEnumerator UltimateRoutine()
    {
        // Блокируем все действия, переходим в ультимейт-стэнс
        agent.isStopped = true;
        anim.SetBool("UltimateStance", true);
        Debug.Log("Ждём аниму перехода");
        CharacterStats stats = player.GetComponent<CharacterStats>();
        Debug.Log("Ждёмс стан");
        stats.ApplyStun();
        // Фаза ожидания: 300 кадров (~5 секунд при 60 FPS)
        yield return new WaitForSeconds(3f);
        
        
        // Проверяем, находится ли игрок на расстоянии не более 7f
        Vector3 horizontalPlayerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        float distanceToPlayer = Vector3.Distance(transform.position, horizontalPlayerPos);
        if (distanceToPlayer <= 10f)
        {
            // Применяем эффекты к игроку
            
            float damageForBleed = stats.armor + 30f;
            stats.StartBleeding(damageForBleed, stats.armor);
            anim.SetTrigger("Ultimate");
            // Фаза ультимейта: 240 кадров (~4 секунды)
            yield return new WaitForSeconds(4f);
            Debug.Log("Анимка атаки");
            // Выходим из ультимейта
            anim.SetBool("UltimateStance", false);
            agent.isStopped = false;
            currentState = State.Follow;
            lastUltimateTime = Time.time;
        }
    }

    // Тестовый корутин для стана моба (настройка через Inspector)
    IEnumerator TestStunRoutine()
    {
        Debug.Log("Test stun activated for " + testStunDuration + " seconds.");
        agent.isStopped = true;
        yield return new WaitForSeconds(testStunDuration);
        agent.isStopped = false;
    }
}
