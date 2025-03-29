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
    public Transform[] patrolPoints; // Патрульные точки
    public float waitTimeAtPatrolPoint = 2f;
    private float distanceToPlayer;

    public WanderingState wanderingState;
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

        // Передаем патрульные точки в патрульное состояние
        if (patrolState != null)
        {
            patrolState.SetPatrolPoints(patrolPoints); // Передаем патрульные точки
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
            if (!(stateMachine.CurrentState is AttackState))
            {
                stateMachine.SetState(attackState);
            }
        }
        else if (distanceToPlayer <= seeEnemyDistance)
        {
            if (!(stateMachine.CurrentState is FollowState))
            {
                stateMachine.SetState(followState);
            }
        }
        else
        {
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                if (!(stateMachine.CurrentState is PatrolState))
                {
                    stateMachine.SetState(patrolState);
                }
            }
            else
            {
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

        if (player != null)
        {
            distanceToPlayer = Vector3.Distance(transform.position, player.position);
        }
        StateSwitch();
        stateMachine.Tick();
        HandleAnimations();
    }

    private void HandleAnimations()
    {
        if (agent.velocity.magnitude > 0.1f)
        {
            anim.SetBool("Walking", true);
        }
        else
        {
            anim.SetBool("Walking", false);
        }
    }
}
