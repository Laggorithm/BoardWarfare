using UnityEngine;
using UnityEngine.UI;

public class ToggleUI : MonoBehaviour
{
    public GameObject hiddenElement;
    public Button showButton;
    public Button hideButton;

    void ShowElement()
    {
        hiddenElement.SetActive(true);
    }
    void HideElement()
    {
        hiddenElement.SetActive(false);
    }
}
