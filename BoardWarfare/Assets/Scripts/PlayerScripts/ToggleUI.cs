using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToggleUI : MonoBehaviour
{
    public GameObject hiddenElement;
    public GameObject Menu;
    public Button showButton;
    public Button hideButton;
    public Movement movement;

    public void ShowElement()
    {
        hiddenElement.SetActive(true);
    }
    public void HideElement()
    {
        hiddenElement.SetActive(false);
    }
    public void ShowMenu()
    {
        Menu.SetActive(true);
    }
    public void HideMenu()
    {
        Menu.SetActive(false);
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void HP()
    {
        Debug.Log("Hp");
        movement.EffectHP();
    }
    public void Def()
    {
        Debug.Log("Def");
        movement.EffectDef();
    }
    public void Speed()
    {
        Debug.Log("Speed");
        movement.EffectSpeed();
    }
    public void Free()
    {
        Debug.Log("Free");
        // effect
    }
}
