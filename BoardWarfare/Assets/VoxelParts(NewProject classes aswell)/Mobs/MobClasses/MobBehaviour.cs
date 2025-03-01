using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MobBehaviour : MonoBehaviour
{
    // Режимы работы моба
    
    public enum State
    {
        Patrol,    // Патруль
        Follow,    // Следование за игроком (вандеринг)
        Attack,    // Атака
        Ultimate,  // Использование ультимейта
        Search,    // Поиск игрока (после потери)
        Retreat    // Отступление (при низком здоровье)
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
    public float attackRange = 5f;

    [Header("Ссылки на объекты")]
    [Tooltip("Ссылка на игрока")]
    public Transform player;

    [Header("Настройки поворота")]
    [Tooltip("Скорость поворота моба")]
    public float rotationSpeed = 5f;

    [Header("Параметры для реакции на удары")]
    [Tooltip("Период между поворотами при получении удара")]
    public float turnCooldown = 0.3f;
    [Tooltip("Длительность эффекта удара (если между ударами прошло больше этого времени, сбрасывается состояние удара)")]
    public float hitDuration = 1f;

    [Header("Настройки ускорения (Follow)")]
    [Tooltip("Исходная скорость агента (будет сохранена автоматически)")]
    public float normalSpeed;
    private Coroutine boostRoutine;

    [Header("Настройки ультимейта")]
    [Tooltip("Минимальное время между использованием ультимейта (в секундах)")]
    public float ultimateCooldown = 10f;
    [Tooltip("Время последнего ультимейта")]
    public float lastUltimateTime = -10f;
    private Coroutine ultimateChanceCoroutine;
    private Coroutine ultimateRoutineCoroutine; // для отмены ультимейта

    [Header("Тестовые настройки")]
    [Tooltip("Если включено, при старте моб будет оглушён (для теста)")]
    public bool testStunOnStart = false;
    [Tooltip("Длительность тестового эффекта стана (в секундах)")]
    public float testStunDuration = 10f;

    // Новые настройки
    [Header("Дополнительное поведение")]
    [Tooltip("Длительность поиска игрока, если его потеряли (в секундах)")]
    public float searchDuration = 3f;
    [Tooltip("Длительность отступления (в секундах)")]
    public float retreatDuration = 2f;
    [Tooltip("Порог здоровья для отступления")]
    public float retreatHealthThreshold = 3f;

    // Внутренние переменные состояния
    private State currentState = State.Patrol;
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool deathStarted = false;
    private float lastHitTime = 0f;
    private bool isTakingHits = false;
    private float lastTurnTime = 0f;
    private bool isUltimateInProgress = false;
    // Флаг, блокирующий смену стейта (перед финальным ожиданием в ультимейте)
    private bool blockStateChange = false;
    // Флаг, указывающий, что ультимейт был отменён ударом
    private bool ultimateCancelled = false;

    // Переменная для хранения последней известной позиции игрока (для состояния Search)
    private Vector3 lastKnownPlayerPos;

    // Компоненты
    private NavMeshAgent agent;
    private Animator anim;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        normalSpeed = agent.speed;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning("Игрок с тегом 'Player' не найден!");
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
            SetDestination(patrolPoints[currentPatrolIndex].position);
        else
            SetDestination(GetRandomPatrolPoint());

        agent.updateRotation = false;
    }

    void Update()
    {
        // Если моб мёртв – гарантированно отключаем движение
        if (isDead)
        {
            agent.ResetPath();
            return;
        }

        // --- Проверки смерти и текущего анимированного состояния ---
        if (health <= 0)
        {
            if (!deathStarted)
            {
                deathStarted = true;
                isDead = true;
                agent.isStopped = true;
                anim.SetTrigger("Die");
                StartCoroutine(Die());
            }
            return;
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Die"))
            return;
        if (currentState == State.Ultimate)
            return;
        if (currentState == State.Attack)
            return;
        if (blockStateChange)
            return;
        // --- Конец проверок ---
        while (health <= 0)
        {
            isTakingHits = false;
            anim.SetBool("IsHit", false);
           
        }
        if (isTakingHits && Time.time - lastHitTime > hitDuration || health <= 0)
        {
            isTakingHits = false;
            anim.SetBool("IsHit", false);
        }

        Vector3 horizontalPlayerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        float distanceToPlayer = Vector3.Distance(transform.position, horizontalPlayerPos);

        // Если в режиме Follow, проверяем условия для отступления (если здоровье низкое)
        if (currentState == State.Follow && health < retreatHealthThreshold && distanceToPlayer < attackRange + 2f)
        {
            SetState(State.Retreat);
            StartCoroutine(RetreatRoutine());
            return;
        }

        // Если моб в режиме Patrol и замечает игрока – переходим в Follow
        if (currentState == State.Patrol && distanceToPlayer <= seeEnemyDistance)
        {
            SetState(State.Follow);
            anim.SetBool("SeeEnemy", true);
            if (isWaiting)
            {
                StopCoroutine(PatrolWait());
                isWaiting = false;
                agent.isStopped = false;
            }
        }
        // Если моб в Follow и теряет игрока, переходим в состояние Search
        else if (currentState == State.Follow && distanceToPlayer > seeEnemyDistance)
        {
            anim.SetBool("SeeEnemy", false);
            lastKnownPlayerPos = horizontalPlayerPos;
            SetState(State.Search);
            StartCoroutine(SearchRoutine());
        }

        // Вызываем методы для управления движением в зависимости от состояния
        if (currentState == State.Patrol)
            Patrol();
        else if (currentState == State.Follow)
            Follow();

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

    // Централизованная смена состояния
    public void SetState(State newState)
    {
        currentState = newState;
    }


    void Patrol()
    {
        // Если агент на месте и не ждёт, запускаем корутину ожидания
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isWaiting)
        {
            StartCoroutine(PatrolWait());
        }
        // Если агент движется, включаем анимацию ходьбы
        else if (!agent.isStopped && agent.velocity.sqrMagnitude > 0.1f)
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
            SetDestination(GetRandomPatrolPoint());
    }

    // Изменён метод: минимальное расстояние теперь 10f
    Vector3 GetRandomPatrolPoint()
    {
        float randomDistance = Random.Range(10f, 15f);
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
        if (ultimateChanceCoroutine == null && !isUltimateInProgress && distanceToPlayer <= attackRange)
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
        if (currentState == State.Attack)
            yield break;

        isAttacking = true;
        SetState(State.Attack);
        agent.isStopped = true;
        CharacterStats stats = player.GetComponent<CharacterStats>();
        anim.SetTrigger("IsAttacking");
        yield return new WaitForSeconds(3f); // Ждем завершения анимации атаки
        agent.isStopped = false;
        isAttacking = false;
        SetState(State.Follow);
    }

    // Если моб находится в ультимейте, метод движения не обновляет направление
    void SetDestination(Vector3 destination)
    {
        if (currentState == State.Ultimate)
            return;

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
        if (currentState != State.Ultimate)
        {
            if (health <= 1 || isDead)
                return;

            lastHitTime = Time.time;
            isTakingHits = true;
            anim.SetTrigger("HitTrigger");
            StartCoroutine(FreezeAgent(1f));
            Vector3 directionToPlayer = new Vector3(player.position.x - transform.position.x, 0, player.position.z - transform.position.z).normalized;
            if (Time.time - lastTurnTime > turnCooldown)
            {
                lastTurnTime = Time.time;
                StartCoroutine(TurnTowardsHit(directionToPlayer));
            }
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
        // Отключаем вандеринг при смерти
        agent.ResetPath();
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
        if (currentState == State.Follow && !isUltimateInProgress && (Time.time - lastUltimateTime >= ultimateCooldown))
        {
            // Активировать ультимейт можно только в Follow (вандеринг)
            if (Random.value <= 0.7f)
            {
                isUltimateInProgress = true;
                ultimateCancelled = false;
                SetState(State.Ultimate);
                agent.isStopped = true;
                agent.ResetPath();
                ultimateRoutineCoroutine = StartCoroutine(UltimateRoutine());
            }
        }
        ultimateChanceCoroutine = null;
    }

    IEnumerator UltimateRoutine()
    {
        Vector3 horizontalPlayerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        float distanceToPlayer = Vector3.Distance(transform.position, horizontalPlayerPos);

        if (distanceToPlayer <= 15f && distanceToPlayer > 4f)
        {
            // Гарантируем, что агент стоит на месте
            agent.isStopped = true;
            agent.ResetPath();

            // При входе в ультимейт оглушаем игрока
            CharacterStats stats = player.GetComponent<CharacterStats>();
            if (stats != null)
                stats.stunDuration = 0.5f;
            stats.ApplyStun();
            stats.stunDuration = 1f;
            // Переход в UltimateStance – моб остаётся неподвижным
            anim.SetBool("UltimateStance", true);
            Debug.Log("Ultimate: переход в стационарную позу");
            yield return new WaitForSeconds(2f);

            // Перед финальным ожиданием блокируем смену стейта
            blockStateChange = true;

            if (stats != null)
            {
                // Повторно оглушаем игрока и применяем эффекты
                anim.SetTrigger("Ultimate");
                Debug.Log("Ultimate: атака");
            }

            yield return new WaitForSeconds(4f); // Ждем завершения анимации ульты
        }
        else
        {
            Debug.Log("Ultimate: отменён, игрок вне досягаемости");
            agent.isStopped = false;
            SetState(State.Follow);
        }

        // Разрешаем смену стейта и завершаем ультимейт
        blockStateChange = false;
        anim.SetBool("UltimateStance", false);
        agent.isStopped = false;
        SetState(State.Follow);
        lastUltimateTime = Time.time;
        isUltimateInProgress = false;
    }

    // Если во время ультимейта моб получает удар, отменяем ультимейт

    // Новая корутина для состояния поиска (Search)
    IEnumerator SearchRoutine()
    {
        // Если моб ещё не дошёл до последней известной позиции игрока – двигаемся туда
        if (Vector3.Distance(transform.position, lastKnownPlayerPos) > agent.stoppingDistance)
        {
            SetDestination(lastKnownPlayerPos);
            while (Vector3.Distance(transform.position, lastKnownPlayerPos) > agent.stoppingDistance)
            {
                // Если игрок внезапно появляется – возвращаемся в преследование
                Vector3 currentPlayerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
                if (Vector3.Distance(transform.position, currentPlayerPos) <= seeEnemyDistance)
                {
                    SetState(State.Follow);
                    yield break;
                }
                yield return null;
            }
        }
        // Достигнув последней известной позиции, моб остаётся в состоянии поиска (Idle) на заданное время
        anim.SetBool("Idle", true);
        anim.SetBool("Walking", false);
        yield return new WaitForSeconds(searchDuration);
        // Если игрок не найден, переходим в патруль
        SetState(State.Patrol);
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        else
            SetDestination(GetRandomPatrolPoint());
    }

    // Новая корутина для состояния отступления (Retreat)
    IEnumerator RetreatRoutine()
    {
        // Определяем направление от игрока и вычисляем точку отступления
        Vector3 retreatDir = (transform.position - new Vector3(player.position.x, transform.position.y, player.position.z)).normalized;
        Vector3 retreatPos = transform.position + retreatDir * 10f;
        SetDestination(retreatPos);
        anim.SetBool("Walking", true);
        anim.SetBool("Idle", false);
        yield return new WaitForSeconds(retreatDuration);
        SetState(State.Follow);
    }

    IEnumerator TestStunRoutine()
    {
        Debug.Log("Test stun activated for " + testStunDuration + " seconds.");
        agent.isStopped = true;
        yield return new WaitForSeconds(testStunDuration);
        agent.isStopped = false;
    }
}
