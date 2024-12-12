using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    public float speed = 6.0f;
    public float rotationSpeed = 360.0f; // Degrees per second
    public float recoilCooldown = 3.0f; // Cooldown for recoil

    public Transform turret;  // Reference to the turret's transform
    public Transform barrelTransform;  // Reference to the barrel's transform
    public float turretRotationSpeed = 100.0f; // Speed of turret rotation
    public float barrelRotationSpeed = 50.0f;  // Speed of barrel rotation
    public Transform shootingPoint;
    public bool IsAiming = false;
    public GameObject projectilePrefab;
    public float shootForce = 100f;
    // Rotation limits
    public float maxBarrelRotation = 10.0f; // Maximum upward rotation in degrees
    public float minBarrelRotation = -12.0f; // Maximum downward rotation in degrees

    private Rigidbody rb;
    private Barrel barrel; // Reference to the Barrel script
    private float nextRecoilTime = 0.0f; // Time when recoil can be applied next

    public float recoilForce = 3.0f; // Adjusted recoil force
    public float forwardForce = 2.0f; // Forward force applied after recoil
    public float rotationTorque = 5.0f; // Torque applied for rotation effect
    public Camera cam1;
    public Camera cam2;
    public float health = 100;
    public float maxHealth = 100;
    public float armor = 20;
    public float armorToughness = 10;  // Placeholder value for armor toughness
    public float critRate = 30.0f;     // Placeholder crit rate
    public float critDamage = 150.0f;  // Placeholder crit damage

    public Image healthBar;
    public TextMeshProUGUI healthText;
    void Start()
    {
       
        rb = GetComponent<Rigidbody>();
        barrel = barrelTransform.GetComponent<Barrel>(); // Get the Barrel script from the barrel transform
        UpdateHealthUI();

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
        if (Input.GetKey(KeyCode.C))
        {
        
            IsAiming = true;
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
     public void Shoot()
    {
        if (shootingPoint != null && projectilePrefab != null)
        {
            // Instantiate the projectile at the shooting point's position and rotation
            GameObject projectile = Instantiate(projectilePrefab, shootingPoint.position, shootingPoint.rotation);

            // Get the Rigidbody component of the projectile to apply force
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Apply a forward force based on the shootForce value
                rb.AddForce(shootingPoint.forward * shootForce, ForceMode.VelocityChange);
            }
        }
        else
        {
            Debug.LogWarning("Shooting point or projectile prefab not assigned.");
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
            Shoot();

            Debug.Log("Recoil applied: Force = " + recoilForce + ", Rotation Torque = " + rotationTorque);
        }
    }

    public void TakeDamage(float damage, float armorStat, float armorToughness, float critRate, float critDamage)
    {
        // Calculate the damage taken after applying armor and toughness
        float finalDamage = damage * (100 / (100 + armorStat * armorToughness));

        // Check for crit chance
        bool isCriticalHit = Random.Range(0f, 1f) <= critRate / 100f;
        if (isCriticalHit)
        {
            finalDamage *= (critDamage / 100f + 1);
            Debug.Log("Critical hit! Damage multiplied by " + (critDamage / 100f + 1));
        }

        // Apply the damage
        health -= finalDamage;

        // Ensure health doesn't go below zero
        health = Mathf.Max(health, 0);

        // Debug log to track health and damage
        Debug.Log($"Damage taken: {finalDamage}. Health: {health}/{maxHealth}");

        UpdateHealthUI();

        // Check if health has dropped to zero or below
        if (health <= 0)
        {
            Destroy(gameObject); // Destroy the object upon death
        }
    }

    // Effect methods (placeholder effects for increasing stats)
    public void EffectHP()
    {
        maxHealth += 10;
        health = Mathf.Min(health + 10, maxHealth);
        Debug.Log($"Health increased to {health}/{maxHealth}");
        UpdateHealthUI();
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
    public void Heal()
    {
        health += 20;
        if (health > maxHealth)
        {
            health = maxHealth;
            Debug.Log("Player HP restored");
        }
        UpdateHealthUI();
        Debug.Log("Player healed!");
    }
    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = health / maxHealth;
            healthText.text = $"{health}/{maxHealth}";
        }
    }
}
