using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;
    public GameObject impactEffect;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;

        Destroy(gameObject, lifetime); // уничтожаем через время
    }

    void OnTriggerEnter(Collider other)
    {
        // Проверка — есть ли у объекта MobBehavior
        MobBehaviour mob = other.GetComponent<MobBehaviour>();

        if (mob != null)
        {
            if (impactEffect != null)
                Instantiate(impactEffect, transform.position, Quaternion.identity);

            Destroy(gameObject); // уничтожаем снаряд
        }
        // Если нет MobBehavior — просто игнорируем и продолжаем лететь
    }
}
