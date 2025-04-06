using UnityEngine;

public abstract class StateBehaviour : MonoBehaviour, IState
{
    protected MobAI mob;
    public virtual bool CanExit => true;

    public virtual void Initialize(MobAI mobAI)
    {
        this.mob = mobAI;
    }

    public abstract void Enter();
    public abstract void Tick();
    public abstract void Exit();
}
