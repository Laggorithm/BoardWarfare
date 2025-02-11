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
    public float waitTimeAtPatrolPoint = 1000000f;

    [Header("Настройки преследования")]
    [Tooltip("Расстояние, на котором моб начинает замечать игрока")]
    public float seeEnemyDistance = 15f;
    [Tooltip("Расстояние до игрока, при котором моб начинает атаку")]
    public float attackRange = 3f;
    // (Если необходимо, можно добавить дистанцию, при которой моб прекращает преследование)

    [Header("Ссылки на объекты")]
    [Tooltip("Ссылка на игрока")]
    public Transform player;

    // Компоненты
    private NavMeshAgent agent;
    private Animator anim;

    // Текущий режим и индекс патрульной точки
    private State currentState = State.Patrol;
    private int currentPatrolIndex = 0;

    // Флаги для контроля состояний
    private bool isWaiting = false;
    private bool isAttacking = false;

    [Header("Настройки поворота")]
    [Tooltip("Скорость поворота моба.")]
    public float rotationSpeed = 5f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // Если ссылка на игрока не задана в инспекторе, ищем его по тегу "Player"
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

        // Если заданы точки патруля, устанавливаем первую цель,
        // иначе выбираем случайную точку
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
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Если моб находится в режиме патруля и игрок попадает в зону обнаружения
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
        // Если моб в режиме преследования, но игрок убежал за пределы зоны видимости
        else if (currentState == State.Follow && distanceToPlayer > seeEnemyDistance)
        {
            currentState = State.Patrol;
            anim.SetBool("SeeEnemy", false);
            // Возвращаемся к патрулю: если заданы точки, выбираем их, иначе генерируем случайную точку
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
    /// Логика патруля:
    /// 1. Моб выбирает точку патруля (либо из массива, либо случайную, если массив пуст)
    /// 2. Поворачивается в сторону цели (реализовано внутри SetDestination)
    /// 3. Передвигается к точке
    /// 4. По достижении – останавливается на заданное время
    /// </summary>
    void Patrol()
    {
        // Если моб не движется (точка достигнута) и не находится в режиме ожидания
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isWaiting)
        {
            StartCoroutine(PatrolWait());
        }
        else
        {
            // Если моб движется, запускаем анимацию ходьбы
            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                anim.SetBool("Walking", true);
                anim.SetBool("Idle", false);
            }
        }
    }

    /// <summary>
    /// Корутина ожидания в точке патруля.
    /// Моб ждёт заданное время, а затем выбирает новую точку: следующую из массива
    /// или случайную, если массив не назначен.
    /// </summary>
    IEnumerator PatrolWait()
    {
        isWaiting = true;
        agent.isStopped = true;
        anim.SetBool("Idle", true);
        anim.SetBool("Walking", false);

        // Ждем указанное время (например, 5 секунд)
        yield return new WaitForSeconds(waitTimeAtPatrolPoint);

        agent.isStopped = false;
        isWaiting = false;

        // Если назначены патрульные точки, переходим к следующей, иначе генерируем случайную точку
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
    /// Генерирует случайную точку для патруля в пределах 5-15 единиц от моба.
    /// </summary>
    Vector3 GetRandomPatrolPoint()
    {
        // Случайное расстояние от 5 до 15
        float randomDistance = Random.Range(5f, 15f);
        // Создаем случайное направление в пределах сферы
        Vector3 randomDirection = Random.insideUnitSphere * randomDistance;
        // Сохраняем текущий уровень по Y
        randomDirection.y = 0;
        Vector3 randomPoint = transform.position + randomDirection;
        return randomPoint;
    }

    /// <summary>
    /// Логика преследования:
    /// 1. Цель – позиция игрока.
    /// 2. Если расстояние до игрока становится <= attackRange, запускается атака.
    /// 3. Если игрок немного дальше – продолжается движение к нему.
    /// </summary>
    void Follow()
    {
        if (isAttacking)
            return; // Пока идёт атака, не обновляем цель

        // Обновляем цель преследования
        SetDestination(player.position);

        // Если игрок достаточно близко – начинаем атаку
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            StartCoroutine(Attack());
        }
        else
        {
            // Обновляем анимации
            anim.SetBool("Walking", true);
            anim.SetBool("Idle", false);
        }
    }

    /// <summary>
    /// Коррутина атаки:
    /// 1. Останавливает движение моба.
    /// 2. Запускает анимацию атаки (триггер "IsAttacking").
    /// 3. Ждёт 3 секунды (время атаки), затем продолжает преследование.
    /// </summary>
    IEnumerator Attack()
    {
        isAttacking = true;
        currentState = State.Attack;
        agent.isStopped = true;

        // Запускаем анимацию атаки
        anim.SetTrigger("IsAttacking");

        // Ждем 3 секунды (время атаки)
        yield return new WaitForSeconds(3f);

        // После атаки возобновляем движение
        agent.isStopped = false;
        isAttacking = false;

        // Если игрок всё ещё рядом, продолжаем преследование (режим Follow)
        currentState = State.Follow;
    }

    /// <summary>
    /// Устанавливает новую цель для NavMeshAgent и выполняет поворот моба в её сторону.
    /// Если flipModel == true, моб будет смотреть в противоположную сторону от цели.
    /// </summary>
    /// <param name="destination">Позиция цели</param>
    void SetDestination(Vector3 destination)
    {
        // Поворот в сторону цели (скорость поворота можно регулировать)
        Vector3 direction = destination - transform.position;
        if (direction != Vector3.zero)
        {
            // Если flipModel включён, разворачиваем моба в противоположную сторону
            Quaternion lookRotation = flipModel ? Quaternion.LookRotation(-direction) : Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

        agent.SetDestination(destination);
    }
}
