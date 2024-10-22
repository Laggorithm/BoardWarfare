using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundAIUnit : MonoBehaviour
{
    private bool IsWandering = true;  // Determine if the unit is in wandering or combat state
    private Vector3 desiredPositionX;  // The position the unit moves towards
    private int rotationSpeed = 150;    // Speed of the unit rotation towards the desired position
    private float wanderRange = 15f;   // Range within which the unit can wander
    private int speed = 5;             // Speed of the unit movement
    private int armor = 20;
    private float Hp = 100;
    public static float Damage = 40;

    void Start()
    {
        StartCoroutine(Wandering());   // Start the wandering coroutine when the game starts
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(IsWandering);
    }

    private IEnumerator Wandering()
    {
        while (IsWandering)
        {
            // Randomize the X and Z positions within a range from the current position
            float randomX = UnityEngine.Random.Range(-wanderRange, wanderRange);
            float randomZ = UnityEngine.Random.Range(-wanderRange, wanderRange);

            // Set the desiredPosition as a random spot around the current position within 15 units
            Vector3 desiredPosition = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
            Debug.Log("New desired position: " + desiredPosition);

            // Calculate the direction and smoothly rotate towards the desired position
            Vector3 direction = desiredPosition - transform.position;
            direction.y = 0;  // Prevent vertical rotation
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

            // Move the unit towards the desired position
            while (Vector3.Distance(transform.position, desiredPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * speed);
                yield return null;  // Wait until the next frame to continue movement
            }

            // Wait for 5 seconds before choosing a new spot
            yield return new WaitForSeconds(5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Test"))
        {
            IsWandering = false; // Stop wandering when entering the trigger
            Vector3 direction = other.gameObject.transform.position - transform.position;
            direction.y = 0;  // Prevent vertical rotation
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Test"))
        {
            IsWandering = true;  // Resume wandering when exiting the trigger
            StartCoroutine(Wandering()); // Restart the wandering coroutine if it was stopped
        }
    }

    public void TakeDamage(float damage)
    {
        float finalDamage = damage * (100 / (100 + armor));
        Hp -= finalDamage;
    }
}
