public class POrderSwitch : POrderBase
{
  private readonly PacType _toType;

  public POrderSwitch(Pac owner, PacType toType) : base(owner)
  {
    _toType = toType;
  }

  public override bool IsCompleted(Context cx) => !Owner.CanUseAbility || Owner.Type == _toType;

  public override bool Execute(Context cx)
  {
    Owner.Switch(_toType);
    return true;
  }
}