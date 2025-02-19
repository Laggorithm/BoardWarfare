using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MobBehaviour : MonoBehaviour
{
    // Режимы работы моба
    private enum State
    {
        Patrol,   // Патруль
        Follow,   // Следование за игроком (вандеринг)
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

        if (testStunOnStart)
            StartCoroutine(TestStunRoutine());
    }

    void Update()
    {
        // Если моб мёртв – гарантированно отключаем движение
        if (isDead)
        {
            agent.ResetPath();
            return;
        }

        // --- Цепочка из 4 проверок ---
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

        if (isTakingHits && Time.time - lastHitTime > hitDuration)
        {
            isTakingHits = false;
            anim.SetBool("IsHit", false);
        }

        Vector3 horizontalPlayerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        float distanceToPlayer = Vector3.Distance(transform.position, horizontalPlayerPos);

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
        else if (currentState == State.Follow && distanceToPlayer > seeEnemyDistance)
        {
            SetState(State.Patrol);
            anim.SetBool("SeeEnemy", false);
            if (patrolPoints != null && patrolPoints.Length > 0)
                SetDestination(patrolPoints[currentPatrolIndex].position);
            else
                SetDestination(GetRandomPatrolPoint());
        }

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
    void SetState(State newState)
    {
        currentState = newState;
    }

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isWaiting)
            StartCoroutine(PatrolWait());
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
            SetDestination(GetRandomPatrolPoint());
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
        //if (stats != null)
            //stats.ApplyStun();
        anim.SetTrigger("IsAttacking");
        yield return new WaitForSeconds(3f);
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
        // Если моб получает удар во время ультимейта, отменяем ультимейт
        if (currentState == State.Ultimate && !ultimateCancelled)
        {
            CancelUltimate();
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
        // Гарантируем, что агент стоит на месте
        agent.isStopped = true;
        agent.ResetPath();

        // При входе в ультимейт оглушаем игрока
        CharacterStats stats = player.GetComponent<CharacterStats>();
        if (stats != null)
            stats.stunDuration = 6f;
            stats.ApplyStun();
        // Переход в UltimateStance – моб остаётся неподвижным
            anim.SetBool("UltimateStance", true);
            Debug.Log("Ultimate: переход в стационарную позу");
            yield return new WaitForSeconds(2f);
        


        // Перед финальным ожиданием блокируем смену стейта
        blockStateChange = true;

        Vector3 horizontalPlayerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        float distanceToPlayer = Vector3.Distance(transform.position, horizontalPlayerPos);
        if (distanceToPlayer <= 10f)
        {
            
            if (stats != null)
            {
                // Повторно оглушаем игрока и применяем эффекты
                
                anim.SetTrigger("Ultimate");
                Debug.Log("Ultimate: атака");
                float damageForBleed = stats.armor + 30f;
                stats.StartBleeding(damageForBleed, stats.armor);
            }
            
            yield return new WaitForSeconds(4f); // Ждем завершения анимации ульты
        }
        else
        {
            Debug.Log("Ultimate: отменён, игрок вне досягаемости");
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
    private void CancelUltimate()
    {
        ultimateCancelled = true;
        if (ultimateRoutineCoroutine != null)
        {
            StopCoroutine(ultimateRoutineCoroutine);
            ultimateRoutineCoroutine = null;
        }
        Debug.Log("Ultimate cancelled due to damage");
        anim.SetBool("UltimateStance", false);
        agent.isStopped = false;
        SetState(State.Follow);
        lastUltimateTime = Time.time;
        isUltimateInProgress = false;
    }

    IEnumerator TestStunRoutine()
    {
        Debug.Log("Test stun activated for " + testStunDuration + " seconds.");
        agent.isStopped = true;
        yield return new WaitForSeconds(testStunDuration);
        agent.isStopped = false;
    }
}
