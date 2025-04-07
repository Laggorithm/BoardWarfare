using UnityEngine;
using System.Collections;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Настройки оружия")]
    public GameObject weaponModel;
    public float attackDamage = 10f;
    public float attackCooldown = 1.0f;
    public SpellHolder playerSpellHolder; // Ссылка на SpellHolder игрока

    private bool isOnCooldown = false;
    private SpellHolder weaponSpellHolder;
    public Sprite weaponIcon; // Спрайт для UI
    public AudioClip attackSound;

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

        // Включаем коллайдер, чтобы он реагировал на столкновения
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f); // Можно настроить радиус
        foreach (var hitCollider in hitColliders)
        {
            MobAI mobAI = hitCollider.GetComponent<MobAI>();
            if (mobAI != null)
            {
                mobAI.TakeDamage((int)attackDamage); // Наносим урон
                Debug.Log($"{name} атакует и наносит {attackDamage} урона!");
            }
        }

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
