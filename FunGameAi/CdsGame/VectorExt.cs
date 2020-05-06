using System;
using System.Numerics;

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