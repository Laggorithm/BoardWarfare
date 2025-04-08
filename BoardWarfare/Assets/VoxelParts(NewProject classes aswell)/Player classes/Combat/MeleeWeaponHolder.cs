using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class MeleeWeaponHolder : MonoBehaviour
{
    [Header("Оружия ближнего боя")]
    [Tooltip("Первое оружие (активируется клавишей 2)")]
    public MeleeWeapon weapon1;

    [Tooltip("Второе оружие (активируется клавишей 3)")]
    public MeleeWeapon weapon2;

    [Tooltip("Ссылка на SpellHolder, который будет отключаться")]
    public SpellHolder spellHolder;

    [Header("UI изображения")]
    [Tooltip("UI-изображение активного оружия")]
    public Image weaponUIImage;

    [Header("Аудио-дорожки")]
    public AudioSource attackSource;

    private Animator animator;
    public AnimatorOverrideController oneHandedOverride;
    public AnimatorOverrideController twoHandedOverride;
    public AnimatorOverrideController scytheOverride;
    public AnimatorOverrideController daggersOverride;
    public AnimatorOverrideController hammerOverride;
    private RuntimeAnimatorController defaultAnimator;


    private MeleeWeapon activeWeapon;

    void Start()
    {
        animator = GetComponent<Animator>();
        defaultAnimator = animator.runtimeAnimatorController;

        if (weapon1 != null) weapon1.gameObject.SetActive(false);
        if (weapon2 != null) weapon2.gameObject.SetActive(false);

        SetActiveWeapon(null);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetActiveWeapon(null);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && weapon1 != null)
        {
            SetActiveWeapon(weapon1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && weapon2 != null)
        {
            SetActiveWeapon(weapon2);
        }

        if (Input.GetMouseButtonDown(0) && activeWeapon != null && activeWeapon.CanAttack())
        {
            activeWeapon.Attack();
            animator.SetTrigger("Attack");
            PlayAttackSound();
        }

    }

    private void SetActiveWeapon(MeleeWeapon newWeapon)
    {
        if (activeWeapon != null)
        {
            activeWeapon.gameObject.SetActive(false);
        }

        activeWeapon = newWeapon;

        if (activeWeapon != null)
        {
            activeWeapon.gameObject.SetActive(true);

            if (activeWeapon.CompareTag("Special") && spellHolder != null)
            {
                spellHolder.enabled = false;
            }
            else if (spellHolder != null)
            {
                spellHolder.enabled = true;
            }

            if (animator != null)
            {
                if (activeWeapon.CompareTag("OneHanded"))
                    animator.runtimeAnimatorController = oneHandedOverride;
                else if (activeWeapon.CompareTag("TwoHanded"))
                    animator.runtimeAnimatorController = twoHandedOverride;
                else if (activeWeapon.CompareTag("Scythe"))
                    animator.runtimeAnimatorController = scytheOverride;
                else if (activeWeapon.CompareTag("Daggers"))
                    animator.runtimeAnimatorController = daggersOverride;
                else if (activeWeapon.CompareTag("Hammer"))
                    animator.runtimeAnimatorController = hammerOverride;
                else
                    animator.runtimeAnimatorController = defaultAnimator;
            }

            // Устанавливаем UI-изображение
            if (weaponUIImage != null && activeWeapon.weaponIcon != null)
            {
                weaponUIImage.sprite = activeWeapon.weaponIcon;
                weaponUIImage.enabled = true;
            }
        }
        else
        {
            if (spellHolder != null) spellHolder.enabled = true;
            if (animator != null) animator.runtimeAnimatorController = defaultAnimator;

            // Отключаем UI-изображение, если оружие убрано
            if (weaponUIImage != null)
            {
                weaponUIImage.enabled = false;
            }
        }
    }
    void PlayAttackSound()
    {
        if (activeWeapon != null && activeWeapon.attackSound != null && attackSource != null)
        {
            attackSource.PlayOneShot(activeWeapon.attackSound);
        }
    }
}
