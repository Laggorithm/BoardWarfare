using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteraction : MonoBehaviour
{
    public string itemName; // Имя предмета, соответствующее ключу в JSON

    private ItemDescriptionManager descriptionManager;
    private bool playerInRange = false;

    void Start()
    {
        descriptionManager = FindObjectOfType<ItemDescriptionManager>();
        if (descriptionManager == null)
        {
            Debug.LogError("ItemDescriptionManager not found in the scene!");
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            descriptionManager.ShowDescription(itemName);
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.Q)) // Нажатие Q для закрытия описания
        {
            descriptionManager.HideDescription();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            descriptionManager.HideDescription();
        }
    }
}
