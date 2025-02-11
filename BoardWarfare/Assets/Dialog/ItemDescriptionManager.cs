using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

[System.Serializable]
public class ItemDescriptionManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text descriptionText;
    public GameObject descriptionPanel;

    [Header("Item Data")]
    private Dictionary<string, string> itemDescriptions;

    void Start()
    {
        descriptionPanel.SetActive(false);
        LoadItemDescriptions("items.json");
    }
    private void LoadItemDescriptions(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            ItemData data = JsonUtility.FromJson<ItemData>(jsonContent);

            itemDescriptions = new Dictionary<string, string>();
            foreach (var item in data.itemList) // Обращение к itemList
            {
                itemDescriptions[item.name] = item.description;
            }

            Debug.Log("Item descriptions loaded successfully!");
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }
    }



    public void ShowDescription(string itemName)
    {
        if (itemDescriptions != null && itemDescriptions.ContainsKey(itemName))
        {
            descriptionText.text = itemDescriptions[itemName];
            descriptionPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Description for item not found: " + itemName);
        }
    }

    public void HideDescription()
    {
        descriptionPanel.SetActive(false);
    }
}