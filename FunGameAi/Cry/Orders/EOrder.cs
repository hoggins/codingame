public abstract class EOrder
{
  protected Entity Robot;

  protected bool IsInitialized;

  protected EOrder(Entity robot)
  {
    Robot = robot;
  }

  public abstract bool IsCompleted(Context cx);

  public abstract string ProduceCommand(Context cx);

  public virtual void Finalize(Context cx)
  {
  }
}