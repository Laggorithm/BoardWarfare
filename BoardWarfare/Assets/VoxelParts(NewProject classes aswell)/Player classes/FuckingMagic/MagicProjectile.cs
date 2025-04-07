using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;
    public GameObject impactEffect;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;

        Destroy(gameObject, lifetime); // ���������� ����� �����
    }

    void OnTriggerEnter(Collider other)
    {
        // �������� � ���� �� � ������� MobBehavior
        MobBehaviour mob = other.GetComponent<MobBehaviour>();

        if (mob != null)
        {
            if (impactEffect != null)
                Instantiate(impactEffect, transform.position, Quaternion.identity);

            Destroy(gameObject); // ���������� ������
        }
        // ���� ��� MobBehavior � ������ ���������� � ���������� ������
    }
}
