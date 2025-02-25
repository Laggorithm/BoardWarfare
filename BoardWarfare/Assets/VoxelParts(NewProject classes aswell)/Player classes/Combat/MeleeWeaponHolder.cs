using UnityEngine;

public class MeleeWeaponHolder : MonoBehaviour
{
    [Header("Оружия ближнего боя")]
    [Tooltip("Первое оружие (активируется клавишей 2)")]
    public MeleeWeapon weapon1;

    [Tooltip("Второе оружие (активируется клавишей 3)")]
    public MeleeWeapon weapon2;

    [Tooltip("Точка для крепления оружия (например, кость руки)")]
    public Transform weaponMountPoint;

    [Tooltip("Ссылка на SpellHolder, который будет отключаться")]
    public SpellHolder spellHolder;

    //Ссылки на смену анимаций
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

        if (Input.GetMouseButtonDown(0) && activeWeapon != null)
        {
            activeWeapon.Attack();
        }
    }

    private void SetActiveWeapon(MeleeWeapon newWeapon)
    {
        if (activeWeapon != null)
        {
            Destroy(activeWeapon.gameObject);
        }

        if (newWeapon != null && weaponMountPoint != null)
        {
            activeWeapon = Instantiate(newWeapon, weaponMountPoint);
            activeWeapon.transform.localPosition = Vector3.zero;
            activeWeapon.transform.localRotation = Quaternion.identity;
            Debug.Log($"Активировано оружие: {activeWeapon.name}");

            if (activeWeapon != null)
            {
                activeWeapon.playerAnimator = GetComponent<Animator>();
            }
            // Проверяем тег экипированного оружия
            if (activeWeapon.CompareTag("Special") && spellHolder != null)
            {
                spellHolder.enabled = false;
                Debug.Log("SpellHolder отключен!");
            }
            else if (spellHolder != null)
            {
                spellHolder.enabled = true;
                Debug.Log("SpellHolder включен!");
            }

            // Меняем анимации в зависимости от оружия
            if (animator != null)
            {
                if (newWeapon.CompareTag("OneHanded"))
                    animator.runtimeAnimatorController = oneHandedOverride;
                else if (newWeapon.CompareTag("TwoHanded"))
                    animator.runtimeAnimatorController = twoHandedOverride;
                else if (newWeapon.CompareTag("Scythe"))
                    animator.runtimeAnimatorController = scytheOverride;
                else if (newWeapon.CompareTag("Daggers"))
                    animator.runtimeAnimatorController = daggersOverride;
                else if (newWeapon.CompareTag("Hammer"))
                    animator.runtimeAnimatorController = hammerOverride;
                else
                    animator.runtimeAnimatorController = null;
            }
        }
        else
        {
            activeWeapon = null;
            Debug.Log("Активирована пустая рука.");
            if (spellHolder != null)
            {
                spellHolder.enabled = true; // Если убираем оружие, включаем SpellHolder обратно
            }
            if (animator != null)
            {
                animator.runtimeAnimatorController = defaultAnimator; // Возвращаем стандартные анимации
            }
        }
    }
}
