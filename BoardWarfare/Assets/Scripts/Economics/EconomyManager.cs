using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public int playerGold = 100; // Золото игрока
    public int enemyGold = 100; // Золото врага
    public TextMeshProUGUI goldText; // UI для золота игрока
    public TextMeshProUGUI goldText2;


    public delegate void GoldChanged(int newGoldAmount);
    public event GoldChanged OnGoldChanged;
    void Start()
    {
        UpdateGoldUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateGoldUI()
    {
        goldText.text = "Gold: " + playerGold;
        goldText2.text = "Gold: " + playerGold;
    }


    public void AddGold(int amount)
    {
        playerGold += amount;
        OnGoldChanged?.Invoke(playerGold);
        UpdateGoldUI();
    }
    public void AddEnemyGold(int amount)
    {
        enemyGold += amount;

    }

    public bool SpendPlayerGold(int amount)
    {
        if (playerGold >= amount)
        {
            playerGold -= amount;
            OnGoldChanged?.Invoke(playerGold);
            UpdateGoldUI();
            return true;
        }
        else
        {
            Debug.Log("Not enough money!");
            return false;
        }
    }

    public bool SpendEnemyGold(int amount)
    {
        if (enemyGold >= amount)
        {
            enemyGold -= amount;
            return true;
        }
        else
        {
            Debug.Log("Enemy doesn't have money!");
            return false;
        }
    }
}
