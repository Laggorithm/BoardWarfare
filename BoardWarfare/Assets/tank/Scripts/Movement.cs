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
    public float armor = 20; // Armor stat
    public float armorToughness = 5; // Armor toughness stat
    public float critRate = 30f; // Player's critical hit rate (out of 100)
    public float critDamage = 150f; // Player's critical damage multiplier (150% = 1.5)

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
        // Calculate damage with armor and armor toughness
        float finalDamage = damage - (armor * armorToughness);

        // Check for critical hit
        if (Random.Range(1, 11) <= critRate / 10) // Critical hit if random number <= crit rate / 10
        {
            finalDamage *= (critDamage / 100 + 1); // Apply critical damage multiplier
            Debug.Log("Critical hit! Damage multiplied by " + (critDamage / 100 + 1));
        }

        // Apply the final damage
        health -= finalDamage;

        // Check if the player is dead
        if (health <= 0)
        {
            Destroy(gameObject); // Destroy the player object upon death
            Debug.Log("Player has died!");
        }
    }
}
