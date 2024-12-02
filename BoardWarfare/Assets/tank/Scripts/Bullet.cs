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

    void OnTriggerEnter(Collider other)
    {
        // Attempt to get the AIUnit component from the collided object
        if (other.TryGetComponent(out AIUnit AIController))
        {
            // Reference to the player (or the object firing the projectile)
            Movement playerMovement = FindObjectOfType<Movement>(); // Finds the Movement component in the scene (the player object)

            // Ensure the playerMovement is found (in case there are multiple objects or none with the Movement script)
            if (playerMovement != null)
            {
                // Check the tag of the collided object and apply the corresponding damage
                switch (other.tag)
                {
                    case "heavy":
                        // Apply damage with player's crit rate and crit damage stats
                        AIController.TakeDamage(damage, AIController.ArmorStat, AIController.ArmorToughness, playerMovement.critRate, playerMovement.critDamage);
                        break;

                    case "ground":
                    case "air":
                        // For ground and air units, we apply damage the same way as for the heavy units
                        AIController.TakeDamage(damage, AIController.ArmorStat, AIController.ArmorToughness, playerMovement.critRate, playerMovement.critDamage);
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

        // Optional: Destroy the projectile if it collides with an object tagged as "ground"
        if (other.CompareTag("ground"))
        {
            Destroy(other.gameObject);
        }
    }


}