public abstract class POrderBase
{
  protected readonly Pac Owner;

  protected POrderBase(Pac owner)
  {
    Owner = owner;
  }

  public abstract bool IsCompleted(Context cx);

  public abstract bool Execute(Context cx);
}