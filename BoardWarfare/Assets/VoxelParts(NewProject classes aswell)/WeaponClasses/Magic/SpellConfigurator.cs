using UnityEngine;
using System;
using System.Collections;

public enum SpellType
{
    SingleShot,
    Burst,
    Shotgun,
    Rectangle
}

[Serializable]
public class SingleShotSettings
{
    [Header("Single Shot Settings")]
    public int projectileCount = 1; // Количество снарядов за один выстрел

    [Header("Spread Settings")]
    public float spreadHorizontal = 15f; // Разброс по горизонтали
    public float spreadVertical = 10f;   // Разброс по вертикали

    
}

[Serializable]
public class BurstSettings
{
    [Header("Burst Settings")]
    public int burstCount = 3; // Количество выстрелов в очереди
    public float burstDelay = 0.1f; // Задержка между выстрелами
}

[Serializable]
public class ShotgunSettings
{
    [Header("Shotgun Settings")]
    public int pelletCount = 5; // Количество пуль за раз
    public float spreadHorizontal = 20f; // Разброс по горизонтали
    public float spreadVertical = 10f;   // Разброс по вертикали
}

[Serializable]
public class RectangleSettings
{
    [Header("Rectangle Settings")]
    public float rectangleLength = 5f;
    public float rectangleHeight = 2f;
    public float rectangleWidth = 10f;
    public float initialDamage = 3f;
    public float spellDuration = 3f;
    [Space]
    public bool enableBlinking = false;
    public int blinkCount = 3;
    public float blinkFrequency = 0.5f;
    public GameObject rectangleObject;
}

public class SpellConfigurator : MonoBehaviour
{
    [Header("Общие настройки заклинания")]
    public SpellType spellType;
    public float cooldown = 1.5f;
    private bool isOnCooldown = false;
    public GameObject projectilePrefab;
    [Space]
    public Transform shootingPoint;

    // Настройки для разных типов стрельбы
    public SingleShotSettings singleShotSettings;
    public BurstSettings burstSettings;
    public ShotgunSettings shotgunSettings;
    public RectangleSettings rectangleSettings;

    private BoxCollider rectangleCollider;

    private void Start()
    {
        if (rectangleSettings != null && rectangleSettings.rectangleObject != null)
        {
            rectangleCollider = rectangleSettings.rectangleObject.GetComponent<BoxCollider>();
            if (rectangleCollider == null)
            {
                Debug.LogError("Rectangle Object не имеет BoxCollider! Добавьте компонент.");
            }
        }

        if (shootingPoint == null)
        {
            Debug.LogWarning("Shooting Point не назначен! Выстрелы будут происходить из позиции объекта.");
            shootingPoint = transform;
        }
    }

    public void CastSpell()
    {
        if (isOnCooldown)
        {
            Debug.Log($"{name} на кулдауне!");
            return;
        }

        switch (spellType)
        {
            case SpellType.SingleShot:
                CastSingleShot();
                break;
            case SpellType.Burst:
                StartCoroutine(CastBurst());
                break;
            case SpellType.Shotgun:
                CastShotgun();
                break;
            case SpellType.Rectangle:
                CastRectangle();
                break;
            default:
                Debug.LogWarning("Неизвестный тип заклинания!");
                break;
        }

        StartCoroutine(StartCooldown());
    }

    #region Методы для каждого типа стрельбы

    /// <summary>
    /// Одиночный выстрел – просто стреляем одним снарядом без разброса.
    /// </summary>
    private void CastSingleShot()
    {
        if (projectilePrefab != null)
        {
            FireProjectile(shootingPoint.rotation);
        }
        else
        {
            Debug.LogWarning("Projectile Prefab не назначен в SingleShotSettings");
        }
    }

    /// <summary>
    /// Burst – стреляем серией выстрелов с заданной задержкой и разбросом.
    /// </summary>
    private IEnumerator CastBurst()
    {
        if (projectilePrefab != null)
        {
            for (int i = 0; i < burstSettings.burstCount; i++)
            {
                Quaternion spreadRotation = GetSpreadRotation(singleShotSettings.spreadHorizontal, singleShotSettings.spreadVertical);
                FireProjectile(spreadRotation);
                yield return new WaitForSeconds(burstSettings.burstDelay);
            }
        }
        else
        {
            Debug.LogWarning("Projectile Prefab не назначен в SingleShotSettings");
        }
    }

    /// <summary>
    /// Shotgun – выстреливает заданное количество пуль с рандомным разбросом.
    /// </summary>
    private void CastShotgun()
    {
        if (projectilePrefab != null)
        {
            for (int i = 0; i < shotgunSettings.pelletCount; i++)
            {
                Quaternion spreadRotation = GetSpreadRotation(shotgunSettings.spreadHorizontal, shotgunSettings.spreadVertical);
                FireProjectile(spreadRotation);
            }
        }
        else
        {
            Debug.LogWarning("Projectile Prefab не назначен (используется SingleShotSettings для Shotgun)");
        }
    }

    /// <summary>
    /// Rectangle – активирует прямоугольный объект с заданными параметрами.
    /// </summary>
    private void CastRectangle()
    {
        if (rectangleSettings != null && rectangleSettings.rectangleObject != null && rectangleCollider != null)
        {
            if (!rectangleSettings.rectangleObject.activeSelf)
                rectangleSettings.rectangleObject.SetActive(true);

            rectangleCollider.size = new Vector3(
                rectangleSettings.rectangleWidth,
                rectangleSettings.rectangleHeight,
                rectangleSettings.rectangleLength
            );

            rectangleCollider.center = new Vector3(0, 0, rectangleSettings.rectangleLength / 2);

            StartCoroutine(HandleRectangleAttack());
        }
        else
        {
            Debug.LogWarning("Rectangle Object не назначен или отсутствует BoxCollider");
        }
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Создаёт и запускает снаряд с указанной ротацией.
    /// </summary>
    private void FireProjectile(Quaternion rotation)
    {
        GameObject projectile = Instantiate(projectilePrefab, shootingPoint.position, rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = rotation * Vector3.forward; // Используем общую скорость для всех типов выстрелов
        }
    }

    /// <summary>
    /// Возвращает рандомную ротацию с заданным разбросом по горизонтали и вертикали.
    /// </summary>
    private Quaternion GetSpreadRotation(float spreadH, float spreadV)
    {
        float randomYaw = UnityEngine.Random.Range(-spreadH, spreadH);
        float randomPitch = UnityEngine.Random.Range(-spreadV, spreadV);
        return Quaternion.Euler(randomPitch, randomYaw, 0) * shootingPoint.rotation;
    }

    /// <summary>
    /// Обрабатывает поведение прямоугольного заклинания (мигание или длительность активации).
    /// </summary>
    private IEnumerator HandleRectangleAttack()
    {
        if (rectangleSettings.enableBlinking)
        {
            bool isActive = true;
            for (int i = 0; i < rectangleSettings.blinkCount; i++)
            {
                rectangleSettings.rectangleObject.SetActive(isActive);
                yield return new WaitForSeconds(rectangleSettings.blinkFrequency);
                isActive = !isActive;
            }
        }
        else
        {
            yield return new WaitForSeconds(rectangleSettings.spellDuration);
        }

        rectangleSettings.rectangleObject.SetActive(false);
    }

    /// <summary>
    /// Запускает кулдаун после активации заклинания.
    /// </summary>
    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }

    #endregion
}
