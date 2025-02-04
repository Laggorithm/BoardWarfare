using UnityEngine;
using System.Collections;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Настройки оружия")]
    public GameObject weaponModel;
    public Animator weaponAnimator;
    public float attackDamage = 10f;
    public float attackCooldown = 1.0f;
    public SpellHolder playerSpellHolder; // Ссылка на SpellHolder игрока

    private bool isOnCooldown = false;
    private SpellHolder weaponSpellHolder;

    private void Start()
    {
        weaponSpellHolder = GetComponent<SpellHolder>();
    }

    public void EquipWeapon()
    {
        if (playerSpellHolder != null && weaponSpellHolder != null)
        {
            playerSpellHolder.enabled = false; // Отключаем SpellHolder у игрока
        }
    }

    public void UnequipWeapon()
    {
        if (playerSpellHolder != null)
        {
            playerSpellHolder.enabled = true; // Включаем обратно при снятии оружия
        }
    }

    public void Attack()
    {
        if (isOnCooldown)
            return;

        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger("Attack");
        }
        else
        {
            Debug.Log($"{name}: запуск атаки без анимации.");
        }

        Debug.Log($"{name} атакует и наносит {attackDamage} урона!");
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        isOnCooldown = false;
    }

    public void ActivateWeapon()
    {
        if (weaponModel != null)
            weaponModel.SetActive(true);
    }

    public void DeactivateWeapon()
    {
        if (weaponModel != null)
            weaponModel.SetActive(false);
    }
}
