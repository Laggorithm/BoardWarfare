using System.Collections.Generic;

public class StateMachine
{
    private IState currentState;
    private Dictionary<IState, List<Transition>> transitions = new Dictionary<IState, List<Transition>>();

    public IState CurrentState => currentState; // Добавляем публичное свойство для текущего состояния

    public void AddState(IState state)
    {
        if (!transitions.ContainsKey(state))
        {
            transitions[state] = new List<Transition>();
        }
    }

    public void AddTransition(IState fromState, IState toState, System.Func<bool> condition)
    {
        if (!transitions.ContainsKey(fromState))
        {
            transitions[fromState] = new List<Transition>();
        }
        transitions[fromState].Add(new Transition(toState, condition));
    }

    public void SetState(IState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }
        currentState = newState;
        currentState.Enter();
    }

    public void Tick()
    {
        if (currentState == null) return;

        foreach (var transition in transitions[currentState])
        {
            if (transition.Condition())
            {
                SetState(transition.ToState);
                return;
            }
        }

        currentState.Tick();
    }

    private class Transition
    {
        public IState ToState { get; }
        public System.Func<bool> Condition { get; }

        public Transition(IState toState, System.Func<bool> condition)
        {
            ToState = toState;
            Condition = condition;
        }
    }
}
