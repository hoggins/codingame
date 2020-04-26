class SOrderPushRoad : SOrderBase
{
  private readonly Path _road;

  public SOrderPushRoad(Squad owner, Path road) : base(owner)
  {
    _road = road;
  }

  public override bool Execute(Context cx)
  {
    return PushRoad(_road);
  }

  private bool PushRoad(Path silkRoad)
  {
    var anyCommands = false;
    foreach (var asset in Owner.Assets)
    {
      var curIdx = silkRoad.FindIndex(asset.NodeId);
      if (curIdx == silkRoad.Count-1)
        continue;
      anyCommands = true;
      var next = silkRoad[curIdx + 1];
      asset.MoveTo(next);
    }

    return anyCommands;
  }
}