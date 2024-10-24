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
}
