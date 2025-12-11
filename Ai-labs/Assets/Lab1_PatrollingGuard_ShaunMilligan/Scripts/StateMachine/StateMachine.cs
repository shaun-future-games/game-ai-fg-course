

public class StateMachine 
{
    // current state
    public IState CurrentState { get; set; }

    public void ChangeState(IState NextState)
    {
        if (CurrentState != null)
        {
            // changing states call exit on the state
            CurrentState.Exit();
        }
        // set the state
        CurrentState = NextState;
        // run logic needed to enter the state if any
        // Null check in case we want to transition to "nothing"
        if (CurrentState != null)
        {
            CurrentState.Enter();
        }
    }
}
