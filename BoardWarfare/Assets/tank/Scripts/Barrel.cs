using UnityEngine;

public class Barrel : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void TriggerRecoil()
    {
        // Trigger the recoil animation
        if (animator != null)
        {
            animator.SetTrigger("Recoil");
        }
    }
}
