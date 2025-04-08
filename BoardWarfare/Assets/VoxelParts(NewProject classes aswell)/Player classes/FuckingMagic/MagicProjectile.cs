using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;
    public GameObject impactEffect;
    public float damage = 10f; // Урон при столкновении

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;

        Destroy(gameObject, lifetime); // Уничтожаем через время
    }

    void OnTriggerEnter(Collider other)
    {
        // Проверка, имеет ли объект тег "USM"
        if (other.CompareTag("USM"))
        {
            // Получаем компонент MobAI с объекта
            MobAI mobAI = other.GetComponent<MobAI>();

            if (mobAI != null)
            {
                // Спавн эффекта при столкновении
                if (impactEffect != null)
                {
                    // Получаем точку столкновения
                    Vector3 spawnPos = other.ClosestPoint(transform.position);
                    GameObject effect = Instantiate(impactEffect, spawnPos, Quaternion.identity);
                    Destroy(effect, 1f); // Эффект исчезнет через 1 секунду
                }

                // Наносим урон
                mobAI.TakeDamage((int)damage);

                // Лог для отладки
                Debug.Log($"{name} атакует {other.name} с тегом USM и наносит {damage} урона!");
            }

            // Уничтожаем снаряд после столкновения
            Destroy(gameObject);
        }
        // Если объект не с тегом "USM", ничего не делаем
    }
}
