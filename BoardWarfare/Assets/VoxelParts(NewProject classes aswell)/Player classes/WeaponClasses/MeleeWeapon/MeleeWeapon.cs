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

    [Tooltip("Время жизни эффекта при попадании")]
    public float hitEffectLifetime = 1.5f;
    public GameObject hitEffectPrefab;

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
    public bool CanAttack()
    {
        return !isOnCooldown;
    }

    public void Attack()
    {
        if (isOnCooldown)
            return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f); // Радиус можно подогнать

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("USM"))
            {
                MobAI mobAI = hitCollider.GetComponent<MobAI>();

                if (mobAI != null)
                {
                    // Спавн эффекта
                    if (hitEffectPrefab != null)
                    {
                        Vector3 spawnPos = hitCollider.ClosestPoint(transform.position);
                        GameObject effect = Instantiate(hitEffectPrefab, spawnPos, Quaternion.identity);
                        Destroy(effect, 1f); // эффект исчезнет через 1 секунду, можешь поменять время
                    }

                    // Наносим урон
                    mobAI.TakeDamage((int)attackDamage);

                    Debug.Log($"{name} атакует {hitCollider.name} с тегом USM и наносит {attackDamage} урона!");
                }
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
