using UnityEngine;

public class SpellHolder : MonoBehaviour
{
    public SpellConfigurator spellQ; // Заклинание на Q
    public SpellConfigurator spellE; // Заклинание на E

    void Update()
    {
        // Активация заклинания на Q
        if (Input.GetKeyDown(KeyCode.Q) && spellQ != null)
        {
            spellQ.CastSpell();
        }

        // Активация заклинания на E
        if (Input.GetKeyDown(KeyCode.E) && spellE != null)
        {
            spellE.CastSpell();
        }
    }
}
