using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SummonSkillState : StateBehaviour
{
    [Header("Summon Settings")]
    public GameObject mobPrefab;
    public int mobCount = 3;
    public float summonDelay = 2f;

    [Header("Animation Settings")]
    public int animationFrameDelay = 200; // ← Кол-во кадров, которые надо подождать

    private bool isSummoning = false;
    private bool isAnimationFinished = false;

    private Animator animator;
    private SpawnManager spawnManager;
    private MobAI mobAI;
    private NavMeshAgent agent;

    public override void Initialize(MobAI mobAI)
    {
        base.Initialize(mobAI);
        this.mobAI = mobAI;
        animator = mobAI.GetComponent<Animator>();
        agent = mobAI.GetComponent<NavMeshAgent>();
        spawnManager = GameObject.FindObjectOfType<SpawnManager>();
    }

    public override void Enter()
    {
        if (isSummoning) return;

        isSummoning = true;
        isAnimationFinished = false;

        if (agent != null)
            agent.isStopped = true;

        if (animator != null)
        {
            Debug.Log("Врубаем скилл");
            animator.SetTrigger("Skill");
        }

        mobAI.StartCoroutine(SummonCoroutine());
    }

    private IEnumerator SummonCoroutine()
    {
        yield return new WaitForSeconds(summonDelay);

        if (spawnManager != null && mobPrefab != null)
        {
            spawnManager.SpawnMobs(mobPrefab, mobCount);
        }

        // Ждём указанное количество кадров перед завершением
        for (int i = 0; i < animationFrameDelay; i++)
        {
            yield return new WaitForEndOfFrame();
        }

        isAnimationFinished = true;
        isSummoning = false;
    }

    public override void Tick()
    {
        // Пока что ничего
    }

    public override void Exit()
    {
        if (agent != null)
            agent.isStopped = false;

        isSummoning = false;
    }

    public override bool CanExit => !isSummoning && isAnimationFinished;
}
