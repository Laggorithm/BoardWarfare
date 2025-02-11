using UnityEngine;
using System;
using System.Collections;

public enum SpellType
{
    SingleShot,
    Rectangle
}

[Serializable]
public class SingleShotSettings
{
    [Header("Single Shot Settings")]
    public float projectileSpeed = 10f;
    public float projectileDamage = 5f;
    public int projectileCount = 1;
    public bool burstMode = false;
    public int burstCount = 3; // Количество выстрелов в бёрсте
    public float burstDelay = 0.1f; // Задержка между выстрелами в бёрсте
    public GameObject projectilePrefab;
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

    [Space]
    public Transform shootingPoint;

    public SingleShotSettings singleShotSettings;
    public RectangleSettings rectangleSettings;

    private BoxCollider rectangleCollider;

    private void Start()
    {
        if (rectangleSettings.rectangleObject != null)
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
            case SpellType.Rectangle:
                CastRectangle();
                break;
        }

        StartCoroutine(StartCooldown());
    }

    private void CastSingleShot()
    {
        if (singleShotSettings.projectilePrefab != null)
        {
            if (singleShotSettings.burstMode)
            {
                StartCoroutine(BurstFire());
            }
            else
            {
                FireProjectile();
            }
        }
        else
        {
            Debug.LogWarning("Projectile Prefab не назначен в SingleShotSettings");
        }
    }


    private IEnumerator BurstFire()
    {
        for (int i = 0; i < singleShotSettings.burstCount; i++)
        {
            FireProjectile();
            yield return new WaitForSeconds(singleShotSettings.burstDelay);
        }
    }


    private void FireProjectile()
    {
        GameObject projectile = Instantiate(singleShotSettings.projectilePrefab, shootingPoint.position, shootingPoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = shootingPoint.forward * singleShotSettings.projectileSpeed;
        }
    }




    private void CastRectangle()
    {
        if (rectangleSettings.rectangleObject != null && rectangleCollider != null)
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

    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }
}