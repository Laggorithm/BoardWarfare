using UnityEngine;

public class MagicHold : MonoBehaviour
{
    public enum SpellType
    {
        SingleShot,
        Cone,
        Beam
    }

    public enum SpellPrefabType
    {
        Single,
        Beam
    }

    [System.Serializable]
    public class SpellSettings
    {
        public SpellType spellType;
        public SpellPrefabType spellPrefab;
        public Transform shootingPoint;
    }

    [Header("Spell Settings")]
    public SpellSettings spellQ;
    public SpellSettings spellE;

    [Header("Spell Prefabs (0 = Single, 1 = Beam)")]
    public Transform[] spellPrefabs = new Transform[2];

    public float fireCooldown = 0.5f;

    private float nextFireTimeQ = 0f;
    private float nextFireTimeE = 0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && Time.time >= nextFireTimeQ)
        {
            CastSpell(spellQ);
            nextFireTimeQ = Time.time + fireCooldown;
        }

        if (Input.GetKeyDown(KeyCode.E) && Time.time >= nextFireTimeE)
        {
            CastSpell(spellE);
            nextFireTimeE = Time.time + fireCooldown;
        }
    }

    void CastSpell(SpellSettings settings)
    {
        switch (settings.spellType)
        {
            case SpellType.SingleShot:
                FireSingleShot(settings);
                break;

            case SpellType.Cone:
                Debug.Log("Cone cast not implemented yet.");
                break;

            case SpellType.Beam:
                FireBeam(settings);
                break;
        }
    }

    void FireSingleShot(SpellSettings settings)
    {
        Transform prefab = GetPrefabByType(settings.spellPrefab);
        if (prefab != null && settings.shootingPoint != null)
        {
            Instantiate(prefab, settings.shootingPoint.position, settings.shootingPoint.rotation);
        }
    }

    void FireBeam(SpellSettings settings)
    {
        Transform prefab = GetPrefabByType(settings.spellPrefab);
        if (prefab != null && settings.shootingPoint != null)
        {
            Instantiate(prefab, settings.shootingPoint.position, settings.shootingPoint.rotation);
        }
    }

    Transform GetPrefabByType(SpellPrefabType type)
    {
        int index = (int)type;
        if (index >= 0 && index < spellPrefabs.Length)
        {
            return spellPrefabs[index];
        }

        Debug.LogWarning("Invalid spell prefab index: " + index);
        return null;
    }
}
