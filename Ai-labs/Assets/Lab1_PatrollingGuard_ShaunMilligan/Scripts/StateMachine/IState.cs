using UnityEngine;

public interface IState
{
    // for things that need to happen when entering the state
    public virtual void Enter() { }
    // for what happens while in the state
    public virtual void Execute() { }
    // for things that need to happen when exiting the state
    public virtual void Exit() { }
}
