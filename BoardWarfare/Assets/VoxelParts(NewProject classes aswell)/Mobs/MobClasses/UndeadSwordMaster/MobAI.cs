using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MobAI : MonoBehaviour
{
    [SerializeField] private List<StateBehaviour> stateBehaviours = new List<StateBehaviour>();
    public StateMachine stateMachine;
    private NavMeshAgent agent;
    private Animator anim;
    public Transform player;

    public float health = 10f;
    public float seeEnemyDistance = 15f;
    public float attackRange = 5f;
    public float retreatHealthThreshold = 3f;
    public Transform[] patrolPoints;
    public float waitTimeAtPatrolPoint = 2f;
    private float distanceToPlayer;

    public WanderingState wanderingState; // Добавляем ссылку на WanderingState
    public PatrolState patrolState;
    public FollowState followState;
    public AttackState attackState;
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        stateMachine = new StateMachine();

        // Создаём состояния из добавленных в инспекторе компонентов
        foreach (var state in stateBehaviours)
        {
            if (state == null)
            {
                Debug.LogWarning("Один из элементов списка stateBehaviours равен null!");
                continue;
            }
            state.Initialize(this);
            stateMachine.AddState(state);
        }

        // Устанавливаем первое состояние, если есть
        if (stateBehaviours.Count > 0)
        {
            stateMachine.SetState(stateBehaviours[0]);
        }
    }

    private void StateSwitch()
    {
        // Проверяем, что текущее состояние не является тем, что мы пытаемся установить
        if (distanceToPlayer <= attackRange)
        {
            // Если игрок в зоне атаки
            if (!(stateMachine.CurrentState is AttackState))
            {
                stateMachine.SetState(attackState);
            }
        }
        else if (distanceToPlayer <= seeEnemyDistance)
        {
            // Если игрок в зоне видимости, но не в зоне атаки
            if (!(stateMachine.CurrentState is FollowState))
            {
                stateMachine.SetState(followState);
            }
        }
        else
        {
            // Если игрок далеко, то начинаем патрулировать или бродить
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                // Если есть патрульные точки, то начинаем патрулировать
                if (!(stateMachine.CurrentState is PatrolState))
                {
                    stateMachine.SetState(patrolState);
                }
            }
            else
            {
                // Если нет патрульных точек, начинаем бродить
                if (!(stateMachine.CurrentState is WanderingState))
                {
                    stateMachine.SetState(wanderingState);
                }
            }
        }
    }




    private void Update()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        // Если игрок существует, проверяем расстояние до него
        if (player != null)
        {
            distanceToPlayer = Vector3.Distance(transform.position, player.position);
        }
        StateSwitch();
        stateMachine.Tick();

        // Управление анимациями в зависимости от скорости
        HandleAnimations();
    }

    private void HandleAnimations()
    {
        if (agent.velocity.magnitude > 0.1f) // Если скорость больше 0.1, начинаем анимацию ходьбы
        {
            anim.SetBool("Walking", true);
        }
        else
        {
            anim.SetBool("Walking", false);
        }
    }
}
