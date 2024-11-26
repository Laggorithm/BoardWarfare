using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    int damage = 40;
    public float lifetime = 5.0f; // Time in seconds before the projectile is destroyed

    void Start()
    {
        // Destroy the projectile after 'lifetime' seconds if it hasn't collided with anything
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        other.TryGetComponent(out AIUnit AIController);
        // Check if the projectile collided with an object tagged as "enemy"
        switch (other.tag)
        {
            case "ground": Object.Destroy(other); break;
            case "heavy": AIController.TakeDamage(damage); break;
        }
    }
}