using UnityEngine;
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

    // Ссылки на смену анимаций
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
        animator = GetComponent<Animator>(); // Инициализируем аниматор
        defaultAnimator = animator.runtimeAnimatorController;

        // Деактивируем оба оружия в начале игры
        if (weapon1 != null) weapon1.gameObject.SetActive(false);
        if (weapon2 != null) weapon2.gameObject.SetActive(false);

        SetActiveWeapon(null); // Начальное состояние — без оружия
    }

    void Update()
    {
        // Выбор оружия по клавишам 1, 2 и 3
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetActiveWeapon(null); // Пустая рука
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && weapon1 != null)
        {
            SetActiveWeapon(weapon1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && weapon2 != null)
        {
            SetActiveWeapon(weapon2);
        }

        // Атака и запуск анимации атаки, если есть активное оружие
        if (Input.GetMouseButtonDown(0) && activeWeapon != null)
        {
            activeWeapon.Attack();         // Логика атаки оружия
            animator.SetTrigger("Attack"); // Запуск анимации атаки
        }
    }

    private void SetActiveWeapon(MeleeWeapon newWeapon)
    {
        // Деактивируем текущее оружие
        if (activeWeapon != null)
        {
            activeWeapon.gameObject.SetActive(false); // Выключаем предыдущее оружие
        }

        activeWeapon = newWeapon; // Устанавливаем новое оружие

        if (activeWeapon != null)
        {
            activeWeapon.gameObject.SetActive(true); // Включаем новое оружие

            // Отключаем SpellHolder при наличии специального оружия
            if (activeWeapon.CompareTag("Special") && spellHolder != null)
            {
                spellHolder.enabled = false;
            }
            else if (spellHolder != null)
            {
                spellHolder.enabled = true;
            }

            // Меняем анимации в зависимости от типа оружия
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
        }
        else
        {
            // Если новое оружие не выбрано, вернуться к базовым настройкам
            if (spellHolder != null) spellHolder.enabled = true;
            if (animator != null) animator.runtimeAnimatorController = defaultAnimator;
        }
    }
}
