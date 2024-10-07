using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public float health = 100f;
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public float moveSpeed = 2f;
    public float armor = 10f;
    //public float lastAttackTime = 0;
    //public float attackCooldown = 1;
    private GameObject target;
    void Start()
    {
        /* Audio
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.volume = 0.5f;*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /*void SelectTarget{}*/

    /*void MoveTowardsTarget()
    {
    }
    /*void AttackTarget()
    {
        // достаточно времени для следующей атаки
        if (lastAttackTime > attackCooldown)
        {
            //урон цели
            target.GetComponent<EnemyController>().TakeDamage(attackDamage);
            lastAttackTime = Time.deltaTime;
        }
    }
    */
    public void TakeDamage(float damage)
    {
        // Рассчитываем урон с учетом брони
        float finalDamage = damage * (100 / (100 + armor));

        // Уменьшаем здоровье при получении урона
        health -= finalDamage;
        //audioSource.PlayOneShot(audioClip);

        if (health <= 0)
        {
            //economyManager.AddEnemyGold(10);
            Destroy(gameObject);
        }
    }
}
