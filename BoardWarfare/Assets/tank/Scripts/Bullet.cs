using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    int damage = 70;
    public float lifetime = 5.0f; // Time in seconds before the projectile is destroyed

    void Start()
    {
        // Destroy the projectile after 'lifetime' seconds if it hasn't collided with anything
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision other)
    {
        // Attempt to get the AIUnit component from the collided object's GameObject
        if (other.gameObject.TryGetComponent(out AIUnit AIController))
        {
            // Reference to the player (or the object firing the projectile)
            Movement playerMovement = FindObjectOfType<Movement>(); // Finds the Movement component in the scene (the player object)

            // Ensure the playerMovement is found (in case there are multiple objects or none with the Movement script)
            if (playerMovement != null)
            {
                // Check the tag of the collided object and apply the corresponding damage
                switch (other.gameObject.tag) // Access the tag via other.gameObject
                {
                    case "Heavy":
                    case "Ground":
                    case "Air":
                        // For ground and air units, we apply damage the same way as for the heavy units
                        Debug.Log("HitEnemy");
                        AIController.TakeDamage(damage, AIController.ArmorStat, AIController.ArmorToughness, playerMovement.critRate, playerMovement.critDamage);
                        Destroy(gameObject); // Destroy the projectile instead of the collided object
                        break;

                    default:
                        // Handle other cases (if any)
                        break;
                }
            }
            else
            {
                Debug.LogWarning("Player Movement not found!");
            }
        }
        else
        {
            Debug.LogWarning("Collided object does not have an AIUnit component.");
        }
    }




}