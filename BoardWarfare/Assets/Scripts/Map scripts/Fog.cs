using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fog : MonoBehaviour
{
    private bool isColliding = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object colliding has the "Player" tag
        if (collision.gameObject.CompareTag("Player"))
        {
            isColliding = true;
            gameObject.SetActive(false); // Disable the box
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Check if the object leaving the collision has the "Player" tag
        if (collision.gameObject.CompareTag("Player"))
        {
            isColliding = false;
            Invoke("ReactivateBox", 0.5f); // Reactivate after a delay
        }
    }

    private void ReactivateBox()
    {
        if (!isColliding)
        {
            gameObject.SetActive(true); // Re-enable the box
        }
    }
}
