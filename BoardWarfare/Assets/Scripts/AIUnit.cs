using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AIUnit : MonoBehaviour
{
    private bool IsWandering;  // Determine if the unit is in wandering or combat state
    private Vector3 desiredPositionX;  // The position the unit moves towards
    private int rotationSpeed = 150;    // Speed of the unit rotation towards the desired position
    private float wanderRange = 15f;   // Range within which the unit can wander
    private int speed;             // Speed of the unit movement
    private int armor;
    private float Hp;
    private float Dmg = 10;
    private float attackRange;
    private int ActionValue = 2;
    private Transform ChosenEnemyUnit;
    private string unitClass;
    void Start()
    {
        InitializeUnitStats();
        Debug.ClearDeveloperConsole();
        IsWandering = true;
        StartCoroutine(Wandering());   // Start the wandering coroutine when the game starts
    }

    // Update is called once per frame
    void Update()
    {

        if (ActionValue == 0)
        {
            Idle();
        }

    }
    private void InitializeUnitStats()
    {
        // Initialize stats based on the unit's class
        unitClass = gameObject.tag; // Assuming the class is defined by the GameObject's tag

        switch (unitClass)
        {
            case "Ground":
                speed = 5;
                armor = 15;
                Hp = 100;
                Dmg = 40;
                attackRange = 5f;
                break;
            case "Air":
                speed = 7; // Air units are faster
                armor = 5; // Less armor
                Hp = 50; // Lower health
                Dmg = 8; // Lower damage
                wanderRange = 20f; // Larger wander range for Air units
                attackRange = 35f;
                break;
            case "Heavy":
                speed = 3; // Heavy units are slower
                armor = 25; // More armor
                Hp = 200; // More health
                Dmg = 50; // More damage
                attackRange = 15f;
                break;
            default:
                Debug.LogWarning("Unit class not recognized. Default stats applied.");
                speed = 5;
                armor = 10;
                Hp = 50;
                Dmg = 5;
                attackRange = 5f;
                break;
        }
    }
    private IEnumerator Wandering()
    {
        while (IsWandering)
        {
            //Walking animation
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
            //idle animation
            // Wait for 5 seconds before choosing a new spot
            yield return new WaitForSeconds(5f);
        }
    }

    private void Idle()
    {
        StopAllCoroutines();
        //idle animation
        //wait for end of animation
    }

    private IEnumerator Approaching()
    {

        while (ChosenEnemyUnit != null && ActionValue > 0)
        {
            // Calculate the direction towards the enemy
            Vector3 direction = ChosenEnemyUnit.position - transform.position;
            direction.y = 0;  // Prevent vertical movement (only rotate on the Y axis)

            // Rotate towards the enemy
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            yield return new WaitForSeconds(2);
            // Calculate the distance between the unit and the chosen enemy unit
            float distanceToEnemy = Vector3.Distance(transform.position, ChosenEnemyUnit.position);
            UnitController unitController = ChosenEnemyUnit.GetComponent<UnitController>();
            // If the unit is within attack range, stop and prepare to attack
            if (distanceToEnemy <= attackRange && ActionValue > 0)
            {
                //attack animation
                Debug.Log("Enemy in range. Ready to attack.");
                unitController.TakeDamage(Dmg);
                Debug.Log("Attacked Enemy Unit");
                ActionValue -= 1;
                //wait for animation to en
                ActionValue = 0;
                yield break;  // Exit the coroutine since no more movement is needed

            }
            if (distanceToEnemy <= attackRange && ActionValue <= 0)
            {
                Debug.Log("Enemy in Range, but no Action Value left");
            }

           

            // Move the unit forward by up to 10 units, scaled by speed and deltaTime
            float moveDistance = Mathf.Min(10f, distanceToEnemy);  // Cap the movement to a maximum of 10 units
            Vector3 forwardMovement = transform.forward * moveDistance * Time.deltaTime * speed;

            // Move the unit forward
            float distanceMoved = 0f;
            while (distanceMoved < 10f && Vector3.Distance(transform.position, ChosenEnemyUnit.position) > attackRange)
            {
                transform.position += forwardMovement;
                distanceMoved += forwardMovement.magnitude;
                yield return null;  // Wait for the next frame
            }

            // Consume one action value for every 10 units traveled
            ActionValue -= 1;
            Debug.Log($"Moved {moveDistance} units towards enemy. Remaining ActionValue: {ActionValue}");

            // Wait for 2 seconds to simulate "thinking"
            yield return new WaitForSeconds(2f);
        }

        // If we run out of action value
        if (ActionValue == 0)
        {
            Debug.Log("No ActionValue left. Waiting for recharge.");
            // Optionally, you can handle what happens while the unit waits for more ActionValue
        }
    }





    private void OnTriggerEnter(Collider other)
    {
        // Check if the other object has the UnitController component
        UnitController enemyUnit = other.GetComponent<UnitController>();

        if (enemyUnit != null)
        {
            // If there's no chosen enemy yet, or this enemy is closer, set it as the chosen one
            if (ChosenEnemyUnit == null)
            {
                ChosenEnemyUnit = other.transform;
                Debug.Log("New enemy detected: " + ChosenEnemyUnit.name);
            }
            else
            {
                // Calculate the distance to the current chosen enemy
                float currentDistance = Vector3.Distance(transform.position, ChosenEnemyUnit.position);
                // Calculate the distance to the new enemy
                float newEnemyDistance = Vector3.Distance(transform.position, other.transform.position);

                // Choose the closer enemy
                if (newEnemyDistance < currentDistance)
                {
                    ChosenEnemyUnit = other.transform;
                    Debug.Log("Switching to a closer enemy: " + ChosenEnemyUnit.name);
                }
            }

            // Stop wandering and start approaching the chosen enemy
            IsWandering = false;
            StopAllCoroutines();
            StartCoroutine(Approaching()); // Immediately approach the chosen enemy
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<UnitController>())
        {
            IsWandering = true;  // Resume wandering when exiting the trigger
            StartCoroutine(Wandering()); // Restart the wandering coroutine if it was stopped
        }
    }

    public void TakeDamage(float damage)
    {
        float finalDamage = damage * (100 / (100 + armor));
        Hp -= finalDamage;
        if (Hp < 0)
        {
            Destroy(gameObject);
        }
    }

}
