using System.Collections.Generic;

class Squad
{
  public SOrderBase Order;
  public List<Asset> Assets = new List<Asset>();

  public void AddAsset(Asset asset)
  {
    Assets.Add(asset);
  }
}