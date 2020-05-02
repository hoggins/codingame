using System;
using System.Numerics;

class Player
{
  static void Main(string[] args)
  {
    var cx = new Context();
    cx.ReadInit();

    // game loop
    while (true)
    {
      cx.ReadTick();

      // Write an action using Console.WriteLine()
      // To debug: Console.Error.WriteLine("Debug messages...");

      for (int i = 0; i < 2; i++)
      {
        var pod = cx.Pods[i];
        var newTarget = Overstear(cx, pod, out var speedAngle);
        Console.Write($"{newTarget.X:0} {newTarget.Y:0} ");
        Console.WriteLine($"{100:0} a{speedAngle:0}");
      }
    }
  }

  private static Vector2 Overstear(Context cx, Pod pod, out double? angle)
  {
    angle = null;

    var node = pod.NextNode(cx);

    if (pod.Velocity.Magnitude() < 50)
      return node.Pos;

    var targetVector = node.Pos - pod.Pos;

    angle = pod.Velocity.Angle(targetVector);
    angle += (angle * 0.15);// * Math.Sign(angle.Value);

    var newT = pod.Velocity.Normalize().Rotate(angle.Value) * targetVector.Magnitude();
    return newT + pod.Pos;
  }
}