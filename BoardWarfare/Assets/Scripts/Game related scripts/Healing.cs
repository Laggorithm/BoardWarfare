using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healing : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Пытаемся получить компонент Movement на объекте, с которым столкнулись
        Movement movement = collision.gameObject.GetComponent<Movement>();

        // Если компонент найден, вызываем метод Heal
        if (movement != null)
        {
            movement.Heal();
            Destroy(gameObject);
        }
    }
}