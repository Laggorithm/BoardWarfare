using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitShop : MonoBehaviour
{
    public GameObject dronePrefab;
    public GameObject standartPrefab;
    public GameObject mechPrefab;

    public int droneCost = 50;
    public int standartCost = 50;
    public int mechCost = 50;

    public GameObject unitPrefab; // Префаб юнита для размещения
    private GameObject selectedCell; // Выбранная клетка для размещения
    public int unitCost; // Стоимость юнита

    private EconomyManager economyManager; // Ссылка на менеджер экономики

    void Start()
    {
        economyManager = FindObjectOfType<EconomyManager>();
        if (economyManager == null)
        {
            Debug.LogError("EconomyManager не найден!");
        }
    }

    // устанавливаем префаб для размещения, но не тратим золото
    public void SelectDrone()
    {
        unitPrefab = dronePrefab;
        unitCost = droneCost; // Устанавливаем стоимость для этого юнита
    }

    public void SelectStandart()
    {
        unitPrefab = standartPrefab;
        unitCost = standartCost; // Устанавливаем стоимость для этого юнита
    }

    public void SelectMech()
    {
        unitPrefab = mechPrefab;
        unitCost = mechCost; // Устанавливаем стоимость для этого юнита
    }

    void Update()
    {
        HandlePlacement();
    }

    void HandlePlacement()
    {
        // клик мыши для размещения юнита
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Проверяем, была ли кликнута клетка
                if (hit.collider.CompareTag("Cell"))
                {
                    selectedCell = hit.collider.gameObject;
                    PlaceUnitOnCell();
                }
            }
        }
    }

    void PlaceUnitOnCell()
    {
        if (selectedCell != null && unitPrefab != null)
        {
            Debug.Log("Попытка размещения юнита на клетке " + selectedCell.name);
            Vector3 position = selectedCell.transform.position;

            // Проверяем, есть ли уже юнит на клетке
            if (selectedCell.GetComponentInChildren<UnitController>() == null)
            {
                // Проверяем, достаточно ли у игрока золота
                if (economyManager.playerGold >= unitCost)
                {
                    // Тратим золото и размещаем юнита
                    economyManager.SpendPlayerGold(unitCost);

                    GameObject newUnit = Instantiate(unitPrefab, position, Quaternion.identity);
                    newUnit.transform.SetParent(selectedCell.transform);

                    Debug.Log("Юнит размещён на клетке за " + unitCost + " золота.");
                }
                else
                {
                    Debug.Log("Недостаточно золота для размещения юнита!");
                }
            }
            else
            {
                Debug.Log("Клетка уже занята!");
            }
        }
        else
        {
            Debug.LogError("Клетка или префаб юнита не выбраны!");
        }
    }
}
