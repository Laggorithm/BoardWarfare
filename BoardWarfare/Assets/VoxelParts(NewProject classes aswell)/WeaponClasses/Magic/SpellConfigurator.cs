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
    public GameObject projectilePrefab;
}

[Serializable]
public class RectangleSettings
{
    [Header("Rectangle Settings")]
    public float initialDamage = 3f;
    public float blinkFrequency = 0.5f;
    public GameObject rectangleObject;
}

public class SpellConfigurator : MonoBehaviour
{
    [Header("Общие настройки заклинания")]
    public SpellType spellType;
    public float cooldown = 1.5f; // Время отката
    private bool isOnCooldown = false;

    [Space]
    public Transform shootingPoint;  // Точка выстрела

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
            shootingPoint = transform; // По умолчанию используется сам объект
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
            for (int i = 0; i < singleShotSettings.projectileCount; i++)
            {
                GameObject projectile = Instantiate(singleShotSettings.projectilePrefab, shootingPoint.position, shootingPoint.rotation);
                Rigidbody rb = projectile.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.velocity = shootingPoint.forward * singleShotSettings.projectileSpeed;
                }
            }
        }
        else
        {
            Debug.LogWarning("Projectile Prefab не назначен в SingleShotSettings");
        }
    }

    public void CastRectangle()
    {
        if (rectangleSettings.rectangleObject != null && rectangleCollider != null)
        {
            // Убедимся, что объект не активен при старте
            if (!rectangleSettings.rectangleObject.activeSelf)
            {
                rectangleSettings.rectangleObject.SetActive(true);
            }

           

            StartCoroutine(HandleRectangleAttack());
        }
        else
        {
            Debug.LogWarning("Rectangle Object не назначен или отсутствует BoxCollider");
        }
    }



    private IEnumerator HandleRectangleAttack()
    {
        float elapsed = 0f;
        float attackDuration = 3f;
        bool isActive = true;

        while (elapsed < attackDuration)
        {
            rectangleSettings.rectangleObject.SetActive(isActive);
            yield return new WaitForSeconds(rectangleSettings.blinkFrequency);
            isActive = !isActive;
            elapsed += rectangleSettings.blinkFrequency;
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
