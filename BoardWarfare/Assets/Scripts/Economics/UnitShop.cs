using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitShop : MonoBehaviour
{
    //  buttons
    [SerializeField] private GameObject[] prefabs;
    public Transform parentObject;

    void Start()
    {
        if (prefabs.Length == 0)
        {
            Debug.LogError("Something is missing mate");
            return;
        }
        ResetCard();
    }
    void ResetCard()
    {
        // Randomly activate one prefab
        int randomIndex = Random.Range(0, prefabs.Length);
        GameObject selectedPrefab = prefabs[randomIndex];

        if (selectedPrefab != null)
        {
            selectedPrefab.SetActive(true);

            // Set the selected prefab's parent and position
            selectedPrefab.transform.SetParent(parentObject);
            selectedPrefab.transform.localPosition = Vector3.zero;
        }
    }
}