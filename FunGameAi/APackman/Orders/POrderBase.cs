public abstract class POrderBase
{
  protected readonly Pac Owner;

  protected POrderBase(Pac owner)
  {
    Owner = owner;
  }

  public abstract bool IsCompleted(Context cx);

  public virtual void Start(Context cx)
  {
  }

  public abstract bool Execute(Context cx);

  public virtual void Complete(Context cx)
  {
  }
}