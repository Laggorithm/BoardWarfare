using System.Collections;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Основные параметры")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float armor = 10f;
    public float incomingDamageReduction = 0f;  // Процент защиты от входящего урона

    [Header("Эффекты")]
    public bool isBleeding = false;
    public float bleedLevel;            // Уровень кровотечения
    public float bleedDuration;         // Длительность эффекта кровотечения
    public float bleedDamagePerSecond;  // Урон в секунду от кровотечения

    [Header("Стан (Stun)")]
    public bool isStunned = false;
    public float stunDuration = 2f;  // Продолжительность эффекта стана в секундах

    private PlayerMovement playerMovement;  // Ссылка на класс PlayerMovement

    void Start()
    {
        currentHealth = maxHealth;
        playerMovement = GetComponent<PlayerMovement>();  // Получаем ссылку на PlayerMovement
    }

    void Update()
    {
        // Применяем кровотечение всегда, независимо от стана
        if (isBleeding)
        {
            ApplyBleedDamage();
        }

        // Если персонаж оглушён, можно дополнительно отключить ввод в PlayerMovement (но не прерывать Update)
        if (isStunned)
        {
            // Например, можно установить нулевое движение или выполнить другую логику,
            // а обработка кровотечения уже идёт выше.
            // playerMovement.HandleMovement(); // Это можно убрать, если хотите просто "заморозить" движение.
        }
    }

    // Метод для применения урона от кровотечения
    private void ApplyBleedDamage()
    {
        // Рассчитываем урон от кровотечения без округления
        bleedDamagePerSecond = maxHealth / 100f * bleedLevel;
        currentHealth -= bleedDamagePerSecond * Time.deltaTime;

        // Можно вывести в Debug, чтобы видеть уменьшение здоровья:
        // Debug.Log("Bleed damage: " + (bleedDamagePerSecond * Time.deltaTime) + " | currentHealth: " + currentHealth);

        // Проверяем, истекла ли длительность кровотечения
        bleedDuration -= Time.deltaTime;
        if (bleedDuration <= 0)
        {
            StopBleeding();
        }
    }

    // Метод для начала кровотечения
    public void StartBleeding(float damage, float defense)
    {
        bleedLevel = Mathf.Round((damage - defense) / 10f);  // Уровень кровотечения зависит от урона и защиты
        bleedDuration = 5f;  // Длительность кровотечения по умолчанию 5 секунд
        isBleeding = true;
    }

    // Метод для остановки кровотечения
    private void StopBleeding()
    {
        isBleeding = false;
        bleedLevel = 0f;
    }

    // Метод для расчета урона с учетом брони и процента защиты
    public float CalculateDamage(float incomingDamage)
    {
        float damageAfterArmor = incomingDamage - (armor * (1 - incomingDamageReduction));
        return Mathf.Round(Mathf.Max(damageAfterArmor, 0));  // Урон не может быть отрицательным, округление
    }

    // Метод для применения эффекта стана
    public void ApplyStun()
    {
        if (isStunned) return;  // Если уже оглушен, не применяем эффект снова

        isStunned = true;
        playerMovement.ApplyStun();  // Передаем команду в PlayerMovement для блокировки движения
        StartCoroutine(RemoveStunAfterDuration());
    }

    private IEnumerator RemoveStunAfterDuration()
    {
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
        playerMovement.RemoveStun();  // Восстанавливаем движение после стана
    }
}
