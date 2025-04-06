using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MobAI : MonoBehaviour
{
    [SerializeField] private List<StateBehaviour> stateBehaviours = new List<StateBehaviour>();
    [SerializeField] private List<StateBehaviour> secondPhaseStateBehaviours = new List<StateBehaviour>(); // Список стейтов для второй фазы

    public StateMachine stateMachine;
    private NavMeshAgent agent;
    private Animator anim;
    public Transform player;

    public float maxHealth; // Новая переменная для максимального хп
    public float health;
    public float seeEnemyDistance;
    public float attackRange;
    public Transform[] patrolPoints;
    public float waitTimeAtPatrolPoint = 2f;
    private float distanceToPlayer;
    public float SecondPhaseAttackRange;
    private int UltimateCharge;
    private int SecondPhaseAdditionalSkillCharge;
    public int Phase = 1;

    public StateBehaviour wanderingState;
    public PatrolState patrolState;
    public StateBehaviour followState;
    public StateBehaviour attackState;

    public StateBehaviour SecondPhaseLightAttack;
    public StateBehaviour SecondPhaseFollow;
    public StateBehaviour SecondPhaseUltimateAttack;
    public StateBehaviour SecondPhaseAdditionalSkill;

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

        // Инициализируем состояния первой фазы
        foreach (var state in stateBehaviours)
        {
            if (state == null) continue;
            state.Initialize(this);
            stateMachine.AddState(state);
        }

        // Инициализируем состояния второй фазы
        foreach (var state in secondPhaseStateBehaviours)
        {
            if (state == null) continue;
            state.Initialize(this);
            stateMachine.AddState(state);
        }

        if (patrolState != null)
        {
            patrolState.SetPatrolPoints(patrolPoints);
        }

        if (stateBehaviours.Count > 0)
        {
            stateMachine.SetState(stateBehaviours[0]);
        }
    }

    private void StateSwitch()
    {
        if (distanceToPlayer <= attackRange)
        {
            // Проверка, если текущий стейт не является attackState
            if (stateMachine.CurrentState != attackState)
            {
                stateMachine.SetState(attackState);
            }
        }
        else if (distanceToPlayer <= seeEnemyDistance)
        {
            // Проверка, если текущий стейт не является followState
            if (stateMachine.CurrentState != followState)
            {
                stateMachine.SetState(followState);
            }
        }
        else
        {
            // Если есть точки патруля
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                // Проверка, если текущий стейт не является patrolState
                if (stateMachine.CurrentState != patrolState)
                {
                    stateMachine.SetState(patrolState);
                }
            }
            else
            {
                // Если нет точек патруля, то проверка на wanderingState
                if (stateMachine.CurrentState != wanderingState)
                {
                    stateMachine.SetState(wanderingState);
                }
            }
        }
    }


    private void StatePhaseTwoSwitch()
    {
        if (distanceToPlayer > SecondPhaseAttackRange)
        {
            if (SecondPhaseAdditionalSkillCharge == 3)
            {
                if (stateMachine.CurrentState != SecondPhaseAdditionalSkill)
                {
                    UltimateCharge += 1;
                    SecondPhaseAdditionalSkillCharge = 0;
                    Debug.Log("Переход во SecondPhaseAdditionalSkill (Summon)");
                    stateMachine.SetState(SecondPhaseAdditionalSkill);
                }
            }
            else
            {
                if (stateMachine.CurrentState != SecondPhaseFollow)
                {
                    Debug.Log("Переход во Follow");
                    stateMachine.SetState(SecondPhaseFollow);
                }
            }
        }
        else
        {
            if (UltimateCharge >= 3)
            {
                UltimateCharge = 0;

                if (stateMachine.CurrentState != SecondPhaseUltimateAttack)
                {
                    Debug.Log("Переход в Ultimate Attack!");
                    stateMachine.SetState(SecondPhaseUltimateAttack);
                }
            }
            else
            {
                if (stateMachine.CurrentState != SecondPhaseLightAttack)
                {
                    SecondPhaseAdditionalSkillCharge += 1;
                    Debug.Log("Переход в Light Attack, заряд навыка: " + SecondPhaseAdditionalSkillCharge);
                    stateMachine.SetState(SecondPhaseLightAttack);
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

        // Переключение фазы при падении хп ниже 50%
        if (Phase == 1 && health <= maxHealth * 0.5f)
        {
            Phase = 2;
        }

        switch (Phase)
        {
            case 1: StateSwitch(); break;
            case 2: StatePhaseTwoSwitch();break;
        }

        stateMachine.Tick();
        HandleAnimations();
    }

    private void HandleAnimations()
    {
        if (agent.velocity.magnitude > 0.1f)
        {
            if (agent.speed == 5)
            {
                anim.SetBool("Walking", true);
            }
            else
            {
                anim.SetBool("Running", true);
            }
        }
        else
        {
            anim.SetBool("Walking", false);
            anim.SetBool("Running", false);
        }
    }
}
