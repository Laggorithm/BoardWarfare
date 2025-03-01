using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UndeadSwordMaster : MonoBehaviour
{
    // ===== ПЕРЕМЕННЫЕ =====
    [Header("Параметры босса")]
    public float maxHealth = 150f;       // 3× базового ХП
    public float health = 150f;
    public float baseSpeed = 30f;        // базовая скорость
    private float currentSpeed;

    // Фазы: "Phase1" (100%–40% ХП) и "Phase2" (менее 40% ХП)
    private string currentState = "Phase1";
    private int dodgeCounter = 0;        // счётчик подряд увертов игрока
    private bool isDead = false;

    [Header("Ссылки на объекты")]
    public GameObject player;            // ссылка на игрока
    [Header("Настройки миньонов")]
    public GameObject knightPrefab;      // префаб призрачного рыцаря
    public List<GameObject> knights = new List<GameObject>();  // список призванных рыцарей

    [Header("Настройки времени и расстояний")]
    public float pauseAfterMiss = 1f;    // время ошеломления после промаха (0.5–1 сек)
    public float teleportInterval = 0.3f; // интервал между телепортациями в Phase2
    public float distanceForWaveAttack = 15f;
    public float distanceForComboAttack = 15f;
    public float phaseTransitionThreshold = 0.4f;  // переход в Phase2, когда ХП < 40%

    // Дополнительные переменные
    private bool caughtInCombo = false;  // флаг – игрок пойман в комбо
    private Coroutine teleportRoutine = null;

    // Компоненты
    private Animator animator;
    private NavMeshAgent agent;

    // ===== МЕТОДЫ =====

    void Start()
    {
        currentState = "Phase1";
        dodgeCounter = 0;
        isDead = false;
        currentSpeed = baseSpeed;

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = currentSpeed;

        // Если игрок не задан, ищем его по тегу "Player"
        if (player == null)
        {
            GameObject temp = GameObject.FindGameObjectWithTag("Player");
            if (temp != null)
                player = temp;
            else
                Debug.LogWarning("Игрок с тегом 'Player' не найден!");
        }

        // Призыв рыцарей (миньонов)
        if (knights.Count == 0)
        {
            StartCoroutine(SummonKnights());
        }

        // Запускаем начальный патруль (движемся к игроку)
        Patrol();
    }

    void Update()
    {
        if (isDead)
        {
            StopAgent();
            return;
        }

        if (health <= 0)
        {
            Die();
            return;
        }

        // Выбор логики в зависимости от фазы
        if (currentState == "Phase1")
        {
            Phase1();
        }
        else if (currentState == "Phase2")
        {
            Phase2();
        }
    }

    // ===== ФАЗА 1 (100% → 40% ХП): Контроль и проверка игрока =====
    void Phase1()
    {
        // Босс патрулирует: медленное движение к игроку
        Patrol();

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        string attackResult = "";

        // Если игрок дальше 15f – используем волну меча, иначе – комбо-атака
        if (distanceToPlayer > distanceForWaveAttack)
        {
            attackResult = WaveAttack();
        }
        else
        {
            attackResult = ComboAttack();
        }

        // Обработка результата атаки
        if (attackResult == "hit")
        {
            dodgeCounter = 0; // сброс счётчика увертов
            TeleportBehindPlayer(); // телепортируемся за спину игрока
            ComboAttack();          // повторная атака

            // При успешном попадании даём команду миньонам атаковать игрока
            CommandMinionsToAttack();
        }
        else // "miss"
        {
            dodgeCounter++;
            if (dodgeCounter >= 2)
            {
                AggressiveRush();   // агрессивный рывок при двух подряд промахах
                dodgeCounter = 0;
                // Команда миньонам перейти в агрессивный режим атаки
                CommandMinionsToAttack();
            }
            else
            {
                StartCoroutine(StunBoss(pauseAfterMiss));
                ActiveChase();

                // Команда миньонам перейти в режим преследования
                CommandMinionsToFollow();
            }
        }

        // Если игрок отступает слишком долго (например, > 3 сек), телепортируемся перед ним и атакуем
        if (GetPlayerRetreatTime() > 3f)
        {
            TeleportInFrontOfPlayer();
            Attack();
        }

        // Переход в Phase2, когда ХП босса опускается ниже 40%
        if (health < maxHealth * phaseTransitionThreshold)
        {
            StopAgent();
            animator.SetTrigger("Scream"); // крик перехода
            KnockbackPlayer(5f);
            currentState = "Phase2";

            // Команда миньонам перейти в режим агрессии
            CommandMinionsToAttack();
        }
    }

    // ===== ФАЗА 2 (40% ХП и ниже): Агрессия и отчаяние =====
    void Phase2()
    {
        IncreaseSpeed();

        if (teleportRoutine == null)
            teleportRoutine = StartCoroutine(TeleportRoutine());

        if (Vector3.Distance(transform.position, player.transform.position) > distanceForComboAttack)
        {
            RushAttack();
        }

        if (dodgeCounter >= 3)
        {
            StartCoroutine(StunBoss(3f)); // длительное ошеломление
            dodgeCounter = 0;
        }

        if (caughtInCombo)
        {
            UltimateAttack();
        }
    }

    // ===== Команды для миньонов =====

    // Команда всем миньонам перейти в режим преследования игрока
    void CommandMinionsToFollow()
    {
        foreach (GameObject knight in knights)
        {
            MobBehaviour mob = knight.GetComponent<MobBehaviour>();
            if (mob != null)
            {
                mob.SetState(MobBehaviour.State.Follow); // Предполагается, что State – public
            }
        }
    }

    // Команда всем миньонам перейти в режим атаки игрока
    void CommandMinionsToAttack()
    {
        foreach (GameObject knight in knights)
        {
            MobBehaviour mob = knight.GetComponent<MobBehaviour>();
            if (mob != null)
            {
                mob.SetState(MobBehaviour.State.Attack);
            }
        }
    }

    // ===== ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ =====

    IEnumerator SummonKnights()
    {
        animator.SetTrigger("SummonKnights");
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 3; i++)
        {
            Vector3 spawnPos = GetSpawnPositionNear(transform.position);
            GameObject knight = Instantiate(knightPrefab, spawnPos, Quaternion.identity);
            knights.Add(knight);
        }

        FaceTowards(player.transform.position);
        animator.SetTrigger("PointAtPlayer");

        // Команда миньонам начать преследование игрока
        CommandMinionsToFollow();

        yield return new WaitForSeconds(3f);
    }

    void Patrol()
    {
        animator.SetBool("Patrol", true);
        agent.speed = baseSpeed * 0.5f;
        agent.SetDestination(player.transform.position);
    }

    string WaveAttack()
    {
        animator.SetTrigger("WaveAttack");
        // Здесь можно создать проджектайл для волны меча.
        // Для симуляции – ждем 3 секунды и определяем результат атаки.
        float playerHPBefore = GetPlayerHP();
        return SimulateAttackResult();
    }

    string ComboAttack()
    {
        animator.SetTrigger("ComboAttack");
        return SimulateAttackResult();
    }

    string SimulateAttackResult()
    {
        return Random.value < 0.7f ? "hit" : "miss";
    }

    void TeleportBehindPlayer()
    {
        Vector3 newPos = GetPositionBehind(player.transform.position);
        Teleport(newPos);
    }

    void TeleportInFrontOfPlayer()
    {
        Vector3 newPos = GetPositionInFront(player.transform.position);
        Teleport(newPos);
    }

    void ActiveChase()
    {
        animator.SetTrigger("ActiveChase");
        agent.speed = baseSpeed;
        agent.SetDestination(player.transform.position);
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        // Здесь добавить логику нанесения урона игроку
    }

    void AggressiveRush()
    {
        animator.SetTrigger("AggressiveRush");
        agent.SetDestination(player.transform.position);
        ComboAttack();
    }

    void RushAttack()
    {
        animator.SetTrigger("RushAttack");
        agent.SetDestination(player.transform.position);
        ComboAttack();
    }

    void Teleport(Vector3 newPosition)
    {
        transform.position = newPosition;
        // Можно добавить визуальные эффекты телепортации здесь
    }

    IEnumerator TeleportRoutine()
    {
        while (currentState == "Phase2" && !isDead)
        {
            yield return new WaitForSeconds(teleportInterval);
            Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
            Vector3 lateral = Vector3.Cross(dirToPlayer, Vector3.up).normalized;
            float side = Random.value < 0.5f ? -1f : 1f;
            Vector3 offset = dirToPlayer * 2f + lateral * 2f;
            Teleport(transform.position + offset);
        }
        teleportRoutine = null;
    }

    void UltimateAttack()
    {
        StartCoroutine(UltimateAttackRoutine());
    }

    IEnumerator UltimateAttackRoutine()
    {
        // Стан игрока через CharacterStats
        CharacterStats cs = player.GetComponent<CharacterStats>();
        if (cs != null)
        {
            cs.stunDuration = 1f;
            cs.ApplyStun();
        }

        animator.SetTrigger("UltimateAttack");
        yield return new WaitForSeconds(0.5f);

        float playerMaxHP = GetPlayerMaxHP();
        float damage = playerMaxHP * 0.3f;
        bool dodged = (Random.value < 0.5f);
        if (dodged)
            damage *= 0.5f;

        if (cs != null)
        {
            cs.currentHealth -= cs.CalculateDamage(damage);
        }

        animator.SetTrigger("SwordThrust");
        KnockbackPlayer(5f);
        health += damage * 0.3f;
        yield return null;
    }

    void IncreaseSpeed()
    {
        agent.speed = baseSpeed * 1.5f;
        animator.SetTrigger("SpeedUp");
    }

    void StopAgent()
    {
        agent.isStopped = true;
    }

    void KnockbackPlayer(float distance)
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 knockDir = (player.transform.position - transform.position).normalized;
            rb.AddForce(knockDir * distance, ForceMode.Impulse);
        }
    }

    float GetPlayerRetreatTime()
    {
        // Здесь можно реализовать определение времени отступления игрока
        return 0f;
    }

    float GetPlayerHP()
    {
        CharacterStats cs = player.GetComponent<CharacterStats>();
        return cs != null ? cs.currentHealth : 100f;
    }

    float GetPlayerMaxHP()
    {
        CharacterStats cs = player.GetComponent<CharacterStats>();
        return cs != null ? cs.maxHealth : 100f;
    }

    Vector3 GetSpawnPositionNear(Vector3 pos)
    {
        return pos + Random.insideUnitSphere * 3f;
    }

    Vector3 GetPositionBehind(Vector3 playerPos)
    {
        Vector3 dir = (playerPos - transform.position).normalized;
        return playerPos - dir * 2f;
    }

    Vector3 GetPositionInFront(Vector3 playerPos)
    {
        Vector3 dir = (playerPos - transform.position).normalized;
        return playerPos + dir * 2f;
    }

    void FaceTowards(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    IEnumerator StunBoss(float duration)
    {
        animator.SetTrigger("Stun");
        agent.isStopped = true;
        yield return new WaitForSeconds(duration);
        agent.isStopped = false;
    }

    void Die()
    {
        isDead = true;
        StopAgent();
        animator.SetTrigger("Die");
        Destroy(gameObject, 4f);
    }
}
