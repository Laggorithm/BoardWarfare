using System.Threading;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed = 6.0f;
    public float rotationSpeed = 360.0f; // Degrees per second
    public float recoilCooldown = 3.0f; // Cooldown for recoil

    public Transform turret;  // Reference to the turret's transform
    public Transform barrelTransform;  // Reference to the barrel's transform
    public float turretRotationSpeed = 100.0f; // Speed of turret rotation
    public float barrelRotationSpeed = 50.0f;  // Speed of barrel rotation

    // Rotation limits
    public float maxBarrelRotation = 10.0f; // Maximum upward rotation in degrees
    public float minBarrelRotation = -12.0f; // Maximum downward rotation in degrees

    private Rigidbody rb;
    private Barrel barrel; // Reference to the Barrel script
    private float nextRecoilTime = 0.0f; // Time when recoil can be applied next

    public float recoilForce = 3.0f; // Adjusted recoil force
    public float forwardForce = 2.0f; // Forward force applied after recoil
    public float rotationTorque = 5.0f; // Torque applied for rotation effect

    public float health = 100;
    public float maxHealth = 100;
    float armor = 20;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        barrel = barrelTransform.GetComponent<Barrel>(); // Get the Barrel script from the barrel transform
        
    }

    void Update()
    {
        // Get input for movement
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Rotate player based on horizontal input
        if (moveX != 0)
        {
            float turn = moveX * rotationSpeed * Time.deltaTime;
            transform.Rotate(0, turn, 0);
        }

        // Move player forward based on vertical input
        Vector3 move = transform.forward * moveZ * speed * Time.deltaTime;
        rb.MovePosition(rb.position + move);

        // Recoil cooldown logic
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.time >= nextRecoilTime)
            {
                nextRecoilTime = Time.time + recoilCooldown;
                ApplyRecoil();
            }
        }

        // Turret Rotation
        if (Input.GetKey(KeyCode.Q))
        {
            RotateTurret(-turretRotationSpeed);
        }
        if (Input.GetKey(KeyCode.E))
        {
            RotateTurret(turretRotationSpeed);
        }

        // Barrel Rotation
        if (Input.GetKey(KeyCode.X))
        {
            RotateBarrel(barrelRotationSpeed);
        }
        if (Input.GetKey(KeyCode.Z))
        {
            RotateBarrel(-barrelRotationSpeed);
        }
    }

    void FixedUpdate()
    {
        // Apply recoil in FixedUpdate for accurate physics
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextRecoilTime)
        {
            ApplyRecoil();
        }
    }

    void RotateTurret(float amount)
    {
        if (turret != null)
        {
            turret.Rotate(Vector3.up, amount * Time.deltaTime);
        }
    }

    void RotateBarrel(float amount)
    {
        if (barrelTransform != null)
        {
            // Calculate new rotation
            float currentRotation = barrelTransform.localEulerAngles.x;
            if (currentRotation > 180) currentRotation -= 360; // Convert to range -180 to 180
            float newRotation = Mathf.Clamp(currentRotation + amount * Time.deltaTime, minBarrelRotation, maxBarrelRotation);

            // Apply the rotation
            barrelTransform.localRotation = Quaternion.Euler(newRotation, barrelTransform.localEulerAngles.y, barrelTransform.localEulerAngles.z);
        }
    }

    void ApplyRecoil()
    {
        if (rb != null && barrel != null)
        {
            // Calculate recoil direction (opposite to the barrel's forward vector)
            Vector3 recoilDirection = -barrelTransform.forward;
            Vector3 recoilRotationDirection = -barrelTransform.right;

            // Apply the recoil force
            rb.AddForce(recoilDirection * recoilForce, ForceMode.Impulse);

            // Apply a rotation torque for visual effect
            rb.AddTorque(recoilRotationDirection * rotationTorque, ForceMode.Impulse);

            // Trigger the recoil animation via the Barrel script
            barrel.TriggerRecoil();

            Debug.Log("Recoil applied: Force = " + recoilForce + ", Rotation Torque = " + rotationTorque);
        }
    }

    public void TakeDamage(float damage)
    {
        // Рассчитываем урон с учетом брони
        float finalDamage = damage * (100 / (100 + armor));

        // Уменьшаем здоровье при получении урона
        health -= finalDamage;

        // Проверяем, умер ли юнит
        if (health <= 0)
        {
            Destroy(gameObject); // Уничтожаем объект при смерти
        }
    }
    public void EffectHP()
    {
        maxHealth += 10;
        health = Mathf.Min(health + 10, maxHealth);
        Debug.Log($"Health increased to {health}/{maxHealth}");
    }

    public void EffectDef()
    {
        armor += 5;
        Debug.Log($"Armor increased to {armor}");
    }

    public void EffectSpeed()
    {
        speed += 2;
        Debug.Log($"Speed increased to {speed}");
    }
}
