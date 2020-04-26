abstract class SOrderBase
{
  protected readonly Squad Owner;

  protected SOrderBase(Squad owner)
  {
    Owner = owner;
  }

  public abstract bool IsCompleted(Context cx);

  public abstract bool Execute(Context cx);
}