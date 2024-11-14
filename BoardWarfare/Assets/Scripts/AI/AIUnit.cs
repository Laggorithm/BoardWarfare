using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIUnit : MonoBehaviour
{
    private int speed;
    private int armor;
    private float Hp;
    private float Dmg = 10;
    private float attackRange;
    private int ActionValue = 2;
    public int cost;
    private Transform chosenEnemy;
    private string unitClass;
    private float spacing;
    private bool hasChosenEnemy = false;

    private List<Transform> detectedEnemies = new List<Transform>();

    void Start()
    {
        InitializeUnitStats();
    }

    private void InitializeUnitStats()
    {
        unitClass = gameObject.tag;

        switch (unitClass)
        {
            case "Ground":
                speed = 5;
                armor = 15;
                Hp = 100;
                Dmg = 40;
                attackRange = 10f;
                cost = 20;
                break;
            case "Air":
                speed = 10;
                armor = 5;
                Hp = 50;
                Dmg = 8;
                cost = 20;
                attackRange = 35f;
                break;
            case "Heavy":
                speed = 3;
                armor = 25;
                Hp = 200;
                Dmg = 50;
                attackRange = 15f;
                cost = 40;
                break;
            default:
                speed = 5;
                armor = 10;
                Hp = 50;
                Dmg = 5;
                attackRange = 5f;
                cost = 20;
                break;
        }
    }

    void Update()
    {
        if (chosenEnemy != null)
        {
            UpdateTargetEnemy();
        }
    }

    private void UpdateTargetEnemy()
    {
        detectedEnemies = detectedEnemies
            .Where(enemy => enemy != null && enemy.TryGetComponent(out UnitController _))
            .ToList();

        if (detectedEnemies.Count == 0)
        {
            chosenEnemy = null;
            return;
        }

        chosenEnemy = detectedEnemies
            .OrderBy(enemy => enemy.GetComponent<UnitController>().health)
            .FirstOrDefault();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out UnitController enemyController))
        {
            if (!detectedEnemies.Contains(other.transform))
            {
                detectedEnemies.Add(other.transform);
                Debug.Log($"Detected enemy: {other.name}");

                UpdateTargetEnemy();
            }
        }
    }

    private void OnDestroy()
    {
        // Handle tile clearing logic here if necessary
    }

    public void TakeDamage(float damage)
    {
        Hp -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage, HP remaining: {Hp}");

        if (Hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has been defeated.");
        Destroy(gameObject, 2f);
    }
}
