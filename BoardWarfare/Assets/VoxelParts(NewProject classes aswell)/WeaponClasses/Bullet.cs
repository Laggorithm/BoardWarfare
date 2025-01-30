using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 50f;
    public float damage = 50f;
    public float lifetime = 2f; // Через сколько секунд уничтожится

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Можно добавить логику урона
        Debug.Log(other.name + " получил " + damage + " урона!");
        Destroy(gameObject);
    }
}
