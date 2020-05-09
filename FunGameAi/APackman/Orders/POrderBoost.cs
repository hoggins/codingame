public class POrderBoost : POrderBase
{
  public POrderBoost(Pac owner) : base(owner)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return !Owner.CanUseAbility || Owner.IsBoosted;
  }

  public override bool Execute(Context cx)
  {
    Owner.Boost();
    return true;
  }
}