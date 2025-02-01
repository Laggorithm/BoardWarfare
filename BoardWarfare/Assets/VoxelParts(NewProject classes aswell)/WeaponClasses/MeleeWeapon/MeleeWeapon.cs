using UnityEngine;
using System.Collections;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Настройки оружия")]
    [Tooltip("Модель оружия, которая показывается при выборе этого оружия")]
    public GameObject weaponModel;

    [Tooltip("Ссылка на аниматор оружия")]
    public Animator weaponAnimator;

    [Tooltip("Урон, наносимый атакой")]
    public float attackDamage = 10f;

    [Tooltip("Кулдаун между атаками")]
    public float attackCooldown = 1.0f;

    private bool isOnCooldown = false;

    /// <summary>
    /// Метод, запускающий атаку оружием.
    /// Если кулдаун не завершён, метод ничего не делает.
    /// </summary>
    public void Attack()
    {
        if (isOnCooldown)
            return;

        // Запускаем анимацию атаки, если аниматор назначен
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger("Attack");
        }
        else
        {
            Debug.Log($"{name}: запуск атаки без анимации.");
        }

        // Здесь можно добавить логику нанесения урона (например, через Raycast или проверку коллизий)
        Debug.Log($"{name} атакует и наносит {attackDamage} урона!");

        StartCoroutine(AttackCooldown());
    }

    /// <summary>
    /// Корутин для реализации кулдауна между атаками.
    /// </summary>
    private IEnumerator AttackCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        isOnCooldown = false;
    }

    /// <summary>
    /// Метод для активации оружия (отображение модели).
    /// </summary>
    public void ActivateWeapon()
    {
        if (weaponModel != null)
            weaponModel.SetActive(true);
    }

    /// <summary>
    /// Метод для деактивации оружия (скрытие модели).
    /// </summary>
    public void DeactivateWeapon()
    {
        if (weaponModel != null)
            weaponModel.SetActive(false);
    }
}
