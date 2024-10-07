using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AICombatBehaviour : MonoBehaviour
{
    private bool PermissionToMove = false;
    private bool IsAttacking;
    private bool IsCapturing;

    private GameObject target;
    private GameObject BaseTile;
    private BoxCollider Fov;

    private int PriorityTargetHP;

    private List<AICombatBehaviour> targetList = new List<AICombatBehaviour>();

    enum Team { Capture, Attack, Defend };
    Team UnitTeam;
    void Start()
    {
        /*
         * check in AICommander script to which team this unit belongs
         * Assigning team where this unit belongs to UnitTeam variable
         */



    }


    void Update()
    {

        if (UnitTeam == Team.Capture)
        {
            MoveToCapture();
        }
        if (UnitTeam == Team.Attack)
        {
            MoveToFight();
        }
        if (UnitTeam == Team.Defend)
        {
            /*
             * Logic for defenders
             */
        }
    }

    void SearchingPriorityTargets()
    {
        /*
        * Look their HP stat
        * Look their Attack stat
        * Find one with lowest HP
        * Find one with highets attack
        * If lowest hp one has below X amount of hp, choose it.
        * If lowest hp one has above X amount of hp, choose one with most attack power
        */




    }





    void MoveToFight()
    {
        if (PermissionToMove == true)
        {
            /*
             * Checking range of movement (on grid in tiles)
             * checking for obstacles in this grid
             * find closest unit controlled by player in that grid
             * Is it a priority one? if yes, attack it. Else, search for priority one. If there are none, attack closest
             */
            if (target != null)
            {
                IsAttacking = true;
            }
            else
            {
                IsAttacking = false;
            }
        }
    }

    public void CheckAndStorePlayerControlledUnit(GameObject gameObject)
    {
        // Check if the game object has the tag "PlayerControlledUnit"
        if (gameObject.CompareTag("PlayerControlledUnit"))
        {
            // Try to get the TargetHealth component
            TargetHealth targetHealth = gameObject.GetComponent<TargetHealth>();

            // Check if the component exists and has an HP variable
            if (targetHealth != null && targetHealth.currentHP > 0)
            {
                // Retrieve HP and position
                float hp = targetHealth.currentHP;
                Vector3 position = gameObject.transform.position;

                // Create a new TargetData object and add it to the list
                TargetData targetData = new TargetData(hp, position);
                targetList.Add(targetData);

                Debug.Log($"PlayerControlledUnit added with HP: {hp}, Position: {position}");
            }
            else
            {
                // The object either does not have TargetHealth or HP is not valid
                Debug.LogWarning("PlayerControlledUnit has no valid HP or TargetHealth component.");
            }
        }
        else
        {
            // The object does not have the correct tag
            Debug.LogWarning("GameObject does not have the PlayerControlledUnit tag.");
        }
    }
    void MoveToCapture()
    {
        if (PermissionToMove == true)
        {
            /*
             * Check position of tiles where you can capture players base
             * set Goal Tile as 1 tile in to grid of Base
             */
            PermissionToMove = false;
            IsCapturing = true;

            if (this.gameObject.transform.position != BaseTile.transform.position)
            {
                IsCapturing = false;
            }
        }

    }
}