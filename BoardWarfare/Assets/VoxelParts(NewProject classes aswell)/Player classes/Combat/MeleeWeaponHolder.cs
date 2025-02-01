using UnityEngine;

public class MeleeWeaponHolder : MonoBehaviour
{
    [Header("Оружия ближнего боя")]
    [Tooltip("Первое оружие (активируется клавишей 2)")]
    public MeleeWeapon weapon1;

    [Tooltip("Второе оружие (активируется клавишей 3)")]
    public MeleeWeapon weapon2;

    // Текущее активное оружие. Если null – пустая рука.
    private MeleeWeapon activeWeapon;

    void Start()
    {
        // При старте активное оружие – пустая рука
        SetActiveWeapon(null);
    }

    void Update()
    {
        // Смена оружия по клавишам 1, 2, 3
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // Пустая рука
            SetActiveWeapon(null);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (weapon1 != null)
                SetActiveWeapon(weapon1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (weapon2 != null)
                SetActiveWeapon(weapon2);
        }

        // Вызов атаки активного оружия по ЛКМ
        if (Input.GetMouseButtonDown(0))
        {
            if (activeWeapon != null)
            {
                activeWeapon.Attack();
            }
            else
            {
                Debug.Log("Пустая рука – атака не выполняется.");
            }
        }
    }

    /// <summary>
    /// Метод для смены активного оружия.
    /// Деактивирует модель предыдущего оружия и активирует новую.
    /// </summary>
    /// <param name="newWeapon">Новое оружие для активации (null – пустая рука).</param>
    private void SetActiveWeapon(MeleeWeapon newWeapon)
    {
        // Деактивируем модель предыдущего оружия, если оно было активно
        if (activeWeapon != null)
        {
            activeWeapon.DeactivateWeapon();
        }

        activeWeapon = newWeapon;

        // Активируем модель нового оружия, если оно не null
        if (activeWeapon != null)
        {
            activeWeapon.ActivateWeapon();
            Debug.Log($"Активировано оружие: {activeWeapon.name}");
        }
        else
        {
            Debug.Log("Активирована пустая рука.");
        }
    }
}
