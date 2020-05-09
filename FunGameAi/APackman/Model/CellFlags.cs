using System;

[Flags]
public enum CellFlags
{
  Wall = 1,
  Space = 1<<2,
  Pellet = 1<<3,
  Visited = 1<<4,
  EnemyPac = 1<<5,
  MyPac = 1<<6,
  Seen = 1<<7,
  Visible = 1<<8,
  HadPellet = 1<<9,
}