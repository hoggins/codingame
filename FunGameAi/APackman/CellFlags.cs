using System;

[Flags]
internal enum CellFlags
{
  Wall = 1,
  Space = 1<<2,
  Pellet = 1<<3,
  Visited = 1<<4,
  EnemyPac = 1<<5,
  MyPac = 1<<6,
}