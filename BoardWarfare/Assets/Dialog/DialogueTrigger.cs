using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DialogueTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        AutoTrigger,    // Автоматический запуск диалога
        InteractTrigger // Запуск диалога по кнопке
    }

    [Header("Trigger Settings")]
    public TriggerType triggerType = TriggerType.AutoTrigger; //выбор запуска диалога
    public int startIndex = 0; // индекс начала диалога в json
    public int endIndex = 1; // конец

    private DialogueManager dialogueManager;
    private bool playerInRange = false; // Флаг для отслеживания нахождения игрока в зоне

    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager doesn't found!");
        }
    }

    void Update()
    {
        // Проверяем нажатие кнопки, если игрок в зоне и тип триггера - InteractTrigger
        if (playerInRange && triggerType == TriggerType.InteractTrigger && Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue(); 
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (triggerType == TriggerType.AutoTrigger)
            {
                StartDialogue();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (triggerType == TriggerType.AutoTrigger)
            {
                dialogueManager?.EndDialogue();
            }
        }
    }

    private void StartDialogue()
    {
        dialogueManager?.StartDialogue(startIndex, endIndex);
    }
}
