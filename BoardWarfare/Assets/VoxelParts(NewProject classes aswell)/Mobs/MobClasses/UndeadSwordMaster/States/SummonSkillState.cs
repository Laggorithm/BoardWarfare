using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SummonSkillState : StateBehaviour
{
    [Header("Summon Settings")]
    public GameObject mobPrefab;
    public int mobCount = 3;
    public float summonDelay = 2f;

    private bool isSummoning = false;
    private Animator animator;
    private SpawnManager spawnManager;
    private MobAI mobAI;

    public override void Initialize(MobAI mobAI)
    {
        base.Initialize(mobAI);
        this.mobAI = mobAI;
        animator = mobAI.GetComponent<Animator>();
        spawnManager = GameObject.FindObjectOfType<SpawnManager>();
    }

    public override void Enter()
    {
        if (isSummoning) return;

        isSummoning = true;

        if (animator != null)
            animator.SetTrigger("Skill");

        mobAI.StartCoroutine(SummonCoroutine());
    }

    private IEnumerator SummonCoroutine()
    {
        yield return new WaitForSeconds(summonDelay);

        if (spawnManager != null && mobPrefab != null)
        {
            spawnManager.SpawnMobs(mobPrefab, mobCount);
        }

        isSummoning = false;
    }

    public override void Tick()
    {
        // Во время анимации ничего не делаем
    }

    public override void Exit()
    {
        isSummoning = false;
    }
}
