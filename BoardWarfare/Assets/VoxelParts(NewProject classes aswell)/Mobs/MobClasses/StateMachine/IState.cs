public interface IState
{
    void Enter();
    void Tick();
    void Exit();
    bool CanExit { get; }
}
