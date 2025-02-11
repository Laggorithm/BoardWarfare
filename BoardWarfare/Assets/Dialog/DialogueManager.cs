using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

[System.Serializable]
public class DialogueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text dialogueText;
    public GameObject dialoguePanel;

    [Header("Dialogue Settings")]
    private DialogueData dialogueData;
    private int currentIndex;
    private int endIndex;

    private Coroutine dialogueCoroutine;
    void Start()
    {
        dialoguePanel.SetActive(false);

        // Загрузка JSON файла
        LoadDialogue("dialogue.json");
        if (dialogueText != null)
        {
            dialogueText.text = "Test text"; // Проверьте, появляется ли текст
        }
        else
        {
            Debug.LogError("dialogueText!");
        }
    }

    void LoadDialogue(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            dialogueData = JsonUtility.FromJson<DialogueData>(jsonContent);
            Debug.LogError("json loaded sucsesfully");
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }
    }

    public void StartDialogue(int startIndex, int endIndex)
    {
        if (dialogueData == null || dialogueData.dialogues.Count == 0)
        {
            Debug.LogError("Dialog not found!");
            return;
        }

        this.currentIndex = startIndex;
        this.endIndex = endIndex;

        dialoguePanel.SetActive(true);

        // Запускаем корутину для последовательного вывода текста
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
        }
        dialogueCoroutine = StartCoroutine(ShowDialogueCoroutine());
    }

    private IEnumerator ShowDialogueCoroutine()
    {
        for (int i = currentIndex; i <= endIndex && i < dialogueData.dialogues.Count; i++)
        {
            dialogueText.text = dialogueData.dialogues[i];
            yield return new WaitForSeconds(3f); // Задержка 3 секунды между строками
        }

        EndDialogue();
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        // Если корутина ещё работает, останавливаем её
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }
    }
}