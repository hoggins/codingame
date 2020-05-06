using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
  static void Print(string s) => Console.Error.WriteLine(s);

  static void MainLanding(string[] args)
  {

    int surfaceN = int.Parse(Console.ReadLine()); // the number of points used to draw the surface of Mars.
    Vector2[] flat = new []{Vector2.One, Vector2.One};
    var lastP = (Vector2?) null;
    for (int i = 0; i < surfaceN; i++)
    {
      var inputs = Console.ReadLine().Split(' ');
      int landX = int.Parse(inputs[0]); // X coordinate of a surface point. (0 to 6999)
      int landY = int.Parse(inputs[1]); // Y coordinate of a surface point. By linking all the points together in a sequential fashion, you form the surface of Mars.
      var newP = new Vector2(landX, landY);
      if (lastP.HasValue)
        if (Math.Abs(landY - lastP.Value.Y) < float.Epsilon)
          flat = new[] {lastP.Value, newP};
      lastP = newP;
    }

    var lendAlt = flat[0].Y;
    flat[0] = new Vector2(flat[0].X + 150, lendAlt);
    flat[1] = new Vector2(flat[1].X - 150, lendAlt);
    var lx = flat[0].X;
    var rx = flat[1].X;
    var midP = new Vector2(lx + (rx - lx) / 2, lendAlt);

    Print(flat[0].X + " " + flat[1].X);

    // game loop
    while (true)
    {
      var inputs = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
      int X = inputs[0];
      int Y = inputs[1];
      int hSpeed = inputs[2]; // the horizontal speed (in m/s), can be negative.
      int vSpeed = inputs[3]; // the vertical speed (in m/s), can be negative.
      int fuel = inputs[4]; // the quantity of remaining fuel in liters.
      int rotate = inputs[5]; // the rotation angle in degrees (-90 to 90).
      int power = inputs[6]; // the thrust power (0 to 4).

      // Write an action using Console.WriteLine()
      // To debug: Console.Error.WriteLine("Debug messages...");

      var down = new Vector2(0,-1);
      var up = new Vector2(0,1);

      var pos = new Vector2(X,Y);
      var velocity = new Vector2(hSpeed, vSpeed-3.711f);

      var toLeft = new Vector2(flat[0].X - X, flat[0].Y - Y);
      var toRight = new Vector2(flat[1].X - X, flat[1].Y - Y);

      var alt = Y - lendAlt;
      var lendIn = alt / vSpeed * -1;
      // var nearest = X < lx ? toRight : toLeft;
      var atTargetIn = (midP.X - X) / hSpeed;

      var nearest = midP - pos;

      // Print($"lend {lendIn} atTarget {atTargetIn} {velocity.Angle(nearest)}");

      nearest = ScaleFromBrakingSpeed(nearest, velocity);
      Print($"nearest {nearest} m{nearest.Magnitude():0}  velocity {velocity} m{velocity.Magnitude():0}");

      var vectorByAng = nearest - velocity;
      Print($"vectorByAng {vectorByAng} myPos {pos}");

      // var ang = CalcVelocityCahange(velocity, nearest);
      // var vectorByAng = up.Rotate(ang);

      // var desireVel = velocity.Rotate(ang);
      // var angle = desireVel.Angle(down);
      // Print($"desireVel {desireVel} angle {angle}");
      // var flipByY = vectorByAng -new Vector2(0, vectorByAng.Y*2);
      var rot = up.Angle(vectorByAng);
      Print($"flip {vectorByAng} rot {rot}");

      // if (Math.Abs(hSpeed) > Math.Abs(vSpeed) && !float.IsInfinity(atTargetIn) && lendIn < atTargetIn)
      // rot = velocity.Angle(nearest)  * (1 - Math.Abs(lendIn) / Math.Abs(atTargetIn));

      if (Y - lendAlt < 300 && atTargetIn > 15)
        rot *= -1;
      if (Y - lendAlt < 300 && atTargetIn < 15)
        rot = 0;

      int thrust;
      if (Math.Abs(rotate - rot) > 100)
        thrust = 0;
      else if (Math.Abs(vSpeed) < 20)
        thrust = 3;
      else
        thrust = 4;

      RunClamped(rot, thrust);


      // rotate power. rotate is the desired rotation angle. power is the desired thrust power.
      //Console.WriteLine("-20 3");
    }
  }

  private static Vector2 ScaleFromBrakingSpeed(Vector2 val, Vector2 velocity)
  {
    var maxBrakeForceY = 3.711f / 4;
    var allowedSpeed = 2;
    var brakeNeeded = velocity.Y * -1 - allowedSpeed;//Math.Abs(velocity.Y * maxBrakeForceY);
    var brakingPossible = Math.Max(0, val.Y / velocity.Y);

    Print($"brakeNeeded {brakeNeeded} brakingPossible {brakingPossible}");

    var brakingY = (brakingPossible - brakeNeeded) * -1;
    var bC = Math.Abs(brakingY / val.Y);
    return new Vector2(val.X * bC, brakingY);

    /*if (brakeNeeded < 0)
      return val;
    if (brakeNeeded < brakingPossible)
      return val;


    var y = val * (brakingPossible -brakeNeeded);

    if (y.Magnitude() < allowedSpeed)
      y = y.Normalize() * 20;
    return y;*/
  }

  private static double CalcVelocityCahange(Vector2 velocity, Vector2 nearest)
  {
    var v = velocity.Magnitude();
    var n = nearest.Magnitude();
    var phi = velocity.Angle(nearest);
    var c = Math.Sqrt(v * v + n * n - 2 * v * n * Math.Cos(phi));

    var ang = Math.Asin(Math.Sin(phi) / c * n) * 180 / Math.PI;

    Print($"v {v}  n {n}  phi {phi}  c {c}  ang {ang}");
    return ang;
  }

  private static void RunClamped(double rot, int thrust)
  {
    rot = Math.Max(-90, Math.Min(90, rot));
    Console.WriteLine($"{rot:0} {thrust:0.#}");
  }
}

internal static class VectorExt
{
  public static float Magnitude(this Vector2 v)
  {
    return Vector2.Distance(Vector2.Zero, v);
  }

  public static Vector2 Normalize(this Vector2 v)
  {
    return Vector2.Normalize(v);
  }

  public static double Angle(this Vector2 vector1, Vector2 vector2)
  {
    double sin = vector1.X * vector2.Y - vector2.X * vector1.Y;
    double cos = vector1.X * vector2.X + vector1.Y * vector2.Y;

    return Math.Atan2(sin, cos) * (180 / Math.PI);
  }

  private const double DegToRad = Math.PI / 180;

  public static Vector2 Rotate(this Vector2 v, double degrees)
  {
    return RotateRadians(v, degrees * DegToRad);
  }

  public static Vector2 RotateRadians(this Vector2 v, double radians)
  {
    var ca = Math.Cos(radians);
    var sa = Math.Sin(radians);
    return new Vector2((float)(ca * v.X - sa * v.Y), (float)(sa * v.X + ca * v.Y));
  }
}