using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;
    public GameObject impactEffect;
    public float damage = 10f; // ���� ��� ������������

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, lifetime); // ���������� ����� �����
    }

    void OnTriggerEnter(Collider other)
    {
        // ��������, ����� �� ������ ��� "USM"
        if (other.CompareTag("USM"))
        {
            // �������� ��������� MobAI � �������
            MobAI mobAI = other.GetComponent<MobAI>();

            if (mobAI != null)
            {
                // ����� ������� ��� ������������
                if (impactEffect != null)
                {
                    // �������� ����� ������������
                    Vector3 spawnPos = other.ClosestPoint(transform.position);
                    GameObject effect = Instantiate(impactEffect, spawnPos, Quaternion.identity);
                    Destroy(effect, 1f); // ������ �������� ����� 1 �������
                }

                // ������� ����
                mobAI.TakeDamage((int)damage);

                // ��� ��� �������
                Debug.Log($"{name} ������� {other.name} � ����� USM � ������� {damage} �����!");
            }

            // ���������� ������ ����� ������������
            Destroy(gameObject);
        }
        // ���� ������ �� � ����� "USM", ������ �� ������
    }
}
