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
    public bool isTarget = false;
    private string PAAATH;

    private GameObject target; // Противник

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

        // Если юнит еще не выбран, игрок может выбрать юнита кликом
        if (!isSelected)
        {
            //  SelectUnit();
        }
        // Если юнит выбран, можно выбрать цель и начать движение
        else if (!isTarget)
        {
            //SelectTarget();

            // Если цель выбрана и у юнита есть действия
            if (target != null && unitActions > 0)
            {
                if (path.Count > 0 && pathIndex < path.Count)
                {
                    FollowPath();
                }
            }
        }
    }
    public void OnGUI()
    {
        GUI.Toggle(new Rect(10, 10, 100, 30), isSelected, "is selected");
        GUI.Toggle(new Rect(10, 20, 100, 30), isTarget, "target selected");
        GUI.Label(new Rect(10, 30, 100, 30), PAAATH);
    }

    public void FindPathToTarget()
    {
        if (target == null) return;

        // Преобразуем позицию юнита и цели в координаты сетки
        int startX = Mathf.RoundToInt(transform.position.x / gridSpawner.scale);
        int startY = Mathf.RoundToInt(transform.position.z / gridSpawner.scale);

        int targetX = Mathf.RoundToInt(target.transform.position.x / gridSpawner.scale);
        int targetY = Mathf.RoundToInt(target.transform.position.z / gridSpawner.scale);

        // Получаем путь от GridSpawner
        path = gridSpawner.GetManhathanPath(startX, startY, targetX, targetY);
        pathIndex = 0;

        if (path.Count == 0)
        {
            Debug.Log("Path not found.");
            PAAATH = "Path not found";
        }
        else
        {
            Debug.Log("Path founded.");
            PAAATH = "Path founded";
        }
    }



    void FollowPath()
    {
        if (path.Count == 0 || pathIndex >= path.Count)
        {
            // Если путь пустой или достигнут конец пути, прекращаем движение
            Debug.Log("Path ended.");
            path.Clear();
            return;
        }

        // Получаем текущую целевую клетку
        GameObject nextTile = path[pathIndex];
        Vector3 nextPosition = nextTile.transform.position;

        // Двигаемся к целевой клетке
        transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);

        // Проверяем, достигнута ли целевая клетка
        if (Vector3.Distance(transform.position, nextPosition) < 0.1f)
        {
            // Обновляем индексы пути и уменьшаем количество доступных действий
            pathIndex++;
            unitActions--;

            // Проверяем, достигнут ли конец пути
            if (pathIndex >= path.Count)
            {
                Debug.Log("Unit on his position.");
                path.Clear(); // Очищаем путь после завершения
            }
        }
    }


    /*void SelectUnit()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                UnitController unit = hit.collider.gameObject.GetComponent<UnitController>();

                if (unit != null && !unit.isSelected)
                {
                    // Сбрасываем предыдущий выбор
                    if (selectedUnit != null)
                    {
                        selectedUnit.ResetUnitSelection();
                    }

                    // Устанавливаем нового выбранного юнита
                    selectedUnit = unit;
                    isSelected = true;
                    unit.isSelected = true;
                    Debug.Log("Unit selected: " + unit.name);
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
                    isTarget = true;
                    // Trigger pathfinding to the target's position
                    FindPathToTarget();
                }
            }
        }
    }*/
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
                    //target.GetComponent<AIUnit>().TakeDamage(attackDamage);

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
        isSelected = false;
        target = null;
        unitActions = 4f; // Сбросим действия для нового хода
    }
}
