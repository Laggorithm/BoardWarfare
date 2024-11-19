using UnityEngine;

public class Wall : MonoBehaviour
{
    public float Health = 40f;

    public void TakeDamage(float damage)
    {
        Health -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage, remaining health: {Health}");

        if (Health <= 0)
        {
            DestroyWall();
        }
    }

    private void DestroyWall()
    {
        Debug.Log($"{gameObject.name} has been destroyed!");
        Destroy(gameObject);
    }
}
