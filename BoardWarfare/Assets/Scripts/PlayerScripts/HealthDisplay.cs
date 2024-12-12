using TMPro;
using UnityEngine;

public class HealthDisplay : MonoBehaviour
{
    public TextMeshProUGUI healthText;  // Reference to the TMP health text field
    public TextMeshProUGUI effectText;  // Reference to the TMP effect text field
    public Movement movement;          // Reference to the Movement script
    public SpawnManager spawnManager;
    private string activeEffect;  // Stores the name of the current effect

    private void Start()
    {

        
        if (movement == null)
        {
            // Automatically find the Movement component if not assigned
            movement = GetComponent<Movement>();
        }

        if (healthText == null)
        {
            Debug.LogError("Health Text is not assigned in HealthDisplay!");
        }

        if (effectText == null)
        {
            Debug.LogError("Effect Text is not assigned in HealthDisplay!");
        }
    }

    private void Update()
    {
        switch (spawnManager.waveEffectValue)
        {
            case 1: activeEffect = $"HP boost, {movement.health}"; break;
            case 2: activeEffect = $"Def Boost, {movement.armor}"; break;
            case 3: activeEffect = "No buff!"; break;
            case 4: activeEffect = "Healed"; break;
            case 0: activeEffect = "None"; break;
        }
        if (movement != null && healthText != null && effectText != null)
        {
            // Update the TMP health text with the current health and max health
            healthText.text = $"{movement.health}/{movement.maxHealth} HP";

            // Update the TMP effect text with the current active effect
            effectText.text = $"Effect: {activeEffect}";
        }
    }

}
