public abstract class BaseState : IState
{
    public virtual void OnStateEnter() { }
    public virtual void OnStateLeave() { }
    public virtual void OnStateOverride() { }
    public virtual void OnStateResume() { }
}
