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
    public float bleedLevel;  // Уровень кровотечения
    public float bleedDuration;  // Длительность эффекта кровотечения
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
        if (isStunned)
        {
            // Блокируем движение, если персонаж оглушен
            playerMovement.HandleMovement();
            return;
        }

        if (isBleeding)
        {
            ApplyBleedDamage();
        }
    }

    // Метод для применения урона от кровотечения
    private void ApplyBleedDamage()
    {
        // Рассчитываем процент урона от кровотечения
        float healthLostPercentage = Mathf.Round(maxHealth / 100 * bleedLevel);
        bleedDamagePerSecond = Mathf.Round(maxHealth / 100 * bleedLevel);

        // Наносим урон
        currentHealth -= Mathf.Round(bleedDamagePerSecond * Time.deltaTime);

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
        bleedLevel = Mathf.Round((damage - defense) / 10);  // Уровень кровотечения зависит от урона и защиты
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
        playerMovement.RemoveStun();  // Ожидаем завершения стана, восстанавливаем движение
    }
}
