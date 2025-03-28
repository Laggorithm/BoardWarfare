using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AttackState : IState
{
    private MobAI mob;
    private NavMeshAgent agent;
    private Animator anim;
    private Transform player;
    private float attackCooldown = 1.5f;
    private float lastAttackTime;

    public AttackState(MobAI mob, NavMeshAgent agent, Animator anim, Transform player)
    {
        this.mob = mob;
        this.agent = agent;
        this.anim = anim;
        this.player = player;
    }

    public void Enter()
    {
        agent.isStopped = true;
        anim.SetTrigger("Attack");
    }

    public void Tick()
    {
        if (Time.time - lastAttackTime > attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    public void Exit()
    {
        agent.isStopped = false;
    }

    private void Attack()
    {
        // Здесь можно добавить логику нанесения урона игроку
        Debug.Log("Mob attacks!");
    }
}
