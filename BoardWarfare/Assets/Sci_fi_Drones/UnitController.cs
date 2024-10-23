using UnityEngine;

public class UnitController : MonoBehaviour
{
    public float maxHealth = 100f;
    public float health = 100f;
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public float moveSpeed = 2f;
    public float armor = 10f;
    public bool hpIsLow = false;
    public bool isSelected = false; // Новый флаг, указывающий, выбран ли юнит
    public float unitActions = 4f;

    private GameObject target; // Противник

    // Статический флаг для состояния игры — определяем, какого юнита выбирает игрок
    public static bool isUnitSelected = false;
    public static UnitController selectedUnit = null;

    void Update()
    {
        // Проверка состояния здоровья
        if (health < maxHealth / 2)
        {
            hpIsLow = true;
        }
        else
        {
            hpIsLow = false;
        }

        // Шаг 1: Если юнит еще не выбран, игрок может выбрать юнита кликом
        if (!isUnitSelected)
        {
            SelectUnit();
        }
        // Шаг 2: Если юнит выбран, игрок может выбрать противника и начать действия
        else if (isSelected)
        {
            SelectTarget();

            // Если цель выбрана и есть доступные действия
            if (target != null && unitActions > 0)
            {
                MoveTowardsTarget();

                // Если юнит находится в радиусе атаки, атакуем
                if (Vector3.Distance(transform.position, target.transform.position) <= attackRange)
                {
                    AttackTarget();
                }
            }
        }
    }

    void SelectUnit()
    {
        // Если игрок нажимает левую кнопку мыши
        if (Input.GetMouseButtonDown(0))
        {
            // Выполняем лучевое сканирование по экрану от позиции курсора
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Если луч попадает в объект
            if (Physics.Raycast(ray, out hit))
            {
                // Проверяем, есть ли у объекта компонент UnitController (другой юнит)
                UnitController unit = hit.collider.gameObject.GetComponent<UnitController>();
                if (unit != null && !unit.isSelected)
                {
                    // Выбираем этого юнита
                    selectedUnit = unit;
                    isUnitSelected = true; // Устанавливаем флаг, что юнит выбран
                    unit.isSelected = true; // Устанавливаем флаг у самого юнита
                    Debug.Log("Unit selected: " + unit.name);
                }
            }
        }
    }

    void SelectTarget()
    {
        // Если игрок нажимает левую кнопку мыши для выбора цели
        if (Input.GetMouseButtonDown(0))
        {
            // Выполняем лучевое сканирование по экрану от позиции курсора
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Если луч попадает в объект
            if (Physics.Raycast(ray, out hit))
            {
                // Проверяем, если у объекта есть компонент EnemyController (враг)
                if (hit.collider.gameObject.GetComponent<UnitController>() != null)
                {
                    target = hit.collider.gameObject; // Устанавливаем цель
                    Debug.Log("Target selected: " + target.name);
                }
            }
        }
    }

    void MoveTowardsTarget()
    {
        // Проверяем, есть ли цель
        if (target != null && unitActions > 0)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;

            // Ограничиваем движение по осям X и Z (движение только вперед, назад, вправо и влево по сетке)
            Vector3 moveDirection = new Vector3(Mathf.Round(direction.x), 0, Mathf.Round(direction.z));

            // Передвигаем юнита в выбранном направлении на одну клетку
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            // Если юнит достиг цели по X и Z, останавливаем движение
            if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                 new Vector3(target.transform.position.x, 0, target.transform.position.z)) < 0.1f)
            {
                Debug.Log("Target reached.");
                unitActions -= 1; // Уменьшаем количество действий
            }
        }
    }

    void AttackTarget()
    {
        // Проверяем, есть ли цель
        if (target != null)
        {
            // Проверяем, находится ли цель в радиусе атаки
            if (Vector3.Distance(transform.position, target.transform.position) <= attackRange)
            {
                // Проверяем, есть ли еще действия у юнита
                if (unitActions > 0)
                {
                    // Наносим урон цели
                    target.GetComponent<AIUnit>().TakeDamage(attackDamage);

                    // Уменьшаем количество действий
                    unitActions -= 1;

                    Debug.Log("Attacked target: " + target.name);
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        // Рассчитываем урон с учетом брони
        float finalDamage = damage * (100 / (100 + armor));

        // Уменьшаем здоровье при получении урона
        health -= finalDamage;

        // Проверяем, умер ли юнит
        if (health <= 0)
        {
            Destroy(gameObject); // Уничтожаем объект при смерти
        }
    }

    public void ResetUnitSelection()
    {
        // Сброс выбора юнита после завершения хода
        isUnitSelected = false;
        isSelected = false;
        target = null;
        unitActions = 4f; // Сбросим действия для нового хода
    }
}
