using System.Collections;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 50f;
    private Vector3 targetPosition;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the rocket collided with an object tagged as "Player"
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get the Movement component of the target (player)
            Movement playerMovement = collision.gameObject.GetComponent<Movement>();

            if (playerMovement != null)
            {
                // Call the TakeDamage method from the Movement class, applying damage
                playerMovement.TakeDamage(damage, playerMovement.armor, playerMovement.armorToughness, playerMovement.critRate, playerMovement.critDamage);
            }

            // Destroy the rocket immediately after dealing damage
            Destroy(gameObject);
        }
    }

    // This method is called when the rocket is spawned
    public void Initialize(Vector3 target)
    {
        targetPosition = target;
        StartCoroutine(MoveTowardsTarget());
    }

    private IEnumerator MoveTowardsTarget()
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

        // Rocket reaches target (if no collision happens before) and destroys itself
        Destroy(gameObject);
    }
}
