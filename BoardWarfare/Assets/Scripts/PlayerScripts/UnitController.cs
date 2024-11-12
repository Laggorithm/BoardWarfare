using System.Collections.Generic;
using TMPro;
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
    public GridSpawner gridSpawner; // Reference to GridSpawner
    private List<GameObject> path = new List<GameObject>(); // Path to follow
    private int pathIndex = 0; // Index in the path

    private GameObject target; // Противник

    // Статический флаг для состояния игры — определяем, какого юнита выбирает игрок
    public static bool isUnitSelected = false;
    public static UnitController selectedUnit = null;

    public TextMeshProUGUI actiontext;
    public TextMeshProUGUI statstext;
    public void Start()
    {
        actiontext.text = "";
    }
    public void Update()
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
            if (target != null && path.Count > 0 && unitActions > 0)
            {
                FollowPath(); // Move along the path
            }
        }
    }
    public void FindPathToTarget()
    {
        // Convert unit's position to grid coordinates
        int startX = Mathf.RoundToInt(transform.position.x / gridSpawner.scale);
        int startY = Mathf.RoundToInt(transform.position.z / gridSpawner.scale);

        // Convert target's position to grid coordinates
        int targetX = Mathf.RoundToInt(target.transform.position.x / gridSpawner.scale);
        int targetY = Mathf.RoundToInt(target.transform.position.z / gridSpawner.scale);

        // Request path from GridSpawner
        path = gridSpawner.GetPath(startX, startY, targetX, targetY);
        pathIndex = 0; // Reset path index to the start
    }

    void FollowPath()
    {
        if (pathIndex < path.Count)
        {
            // Get the next position from the path
            GameObject nextTile = path[pathIndex];
            Vector3 nextPosition = nextTile.transform.position;

            // Move toward the next tile position
            transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);

            // Check if reached the tile
            if (Vector3.Distance(transform.position, nextPosition) < 0.1f)
            {
                pathIndex++; // Move to the next tile in the path
                unitActions--; // Reduce available actions for the unit
            }
        }
        else
        {
            Debug.Log("Reached target through path.");
            path.Clear(); // Clear path once target is reached
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
                    statstext.text = "";
                }
            }
        }
    }

    void SelectTarget()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.GetComponent<AIUnit>() != null)
                {
                    target = hit.collider.gameObject;
                    Debug.Log("Target selected: " + target.name);
                    statstext.text = "";

                    // Trigger pathfinding to the target's position
                    FindPathToTarget();
                }
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
                    actiontext.text = "";
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
