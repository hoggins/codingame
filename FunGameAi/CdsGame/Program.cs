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
  private static Vector2? _eLastPos;
  private static Vector2 _ePos;
  private static Vector2? _eVelocity;


  private static Vector2? _lastPos;

  private static bool IsBoostUsed;
  private static Vector2 _myPos;
  private static Vector2? _myVelocity;
  private static int _speed;

  private static Vector2 _targetPos;

  static string Src(string editor)
  {
//    return editor;
    return Console.ReadLine();
  }

  static void Main(string[] args)
  {
    // game loop
    while (true)
    {
      ReadInputs(out var nextCheckpointDist, out var nextCheckpointAngle);
      // Write an action using Console.WriteLine()
      // To debug: Console.Error.WriteLine("Debug messages...");


      // You have to output the target position
      // followed by the power (0 <= thrust <= 100)
      // i.e.: "x y thrust"


      var thrust = 100d;

      var breakClosing = Vector2.Distance(_myPos, _targetPos) < Vector2.Distance(_targetPos, _ePos)
        ? CalcBreakClosing(nextCheckpointDist)
        : 0;


      // being too accurate is not effective when you collide with opponent
      var breakTurn = _speed < 300 || nextCheckpointDist > 5000
        ? CalcBreakTurning(nextCheckpointAngle, out var angle)
        : CalcBreakTurningSpeed(_myPos, _targetPos, out angle);
      //thrust -= breakTurn;


      thrust = Math.Max(0, thrust);

      var newTarget = Overstear(out var speedAngle);
      Console.Write($"{newTarget.X:0} {newTarget.Y:0} ");

      if (!IsBoostUsed && speedAngle < 10 && angle < 10 && nextCheckpointDist > 5000)
      {
        Console.WriteLine("BOOST");
        IsBoostUsed = true;
      }
      else
        Console.WriteLine($"{thrust:0} a{speedAngle:0} b{breakClosing}");

      _lastPos = _myPos;
      _eLastPos = _ePos;
    }
  }

  private static void ReadInputs(out int nextCheckpointDist, out int nextCheckpointAngle)
  {
    string[] inputs;
    var src1 = "3437 7241" +
               " 9425 7263" +
               " 500" +
               " 2000";
    var src2 = "0 0";

    inputs = Src(src1).Split(' ');
    int x = int.Parse(inputs[0]);
    int y = int.Parse(inputs[1]);
    int nextCheckpointX = int.Parse(inputs[2]); // x position of the next check point
    int nextCheckpointY = int.Parse(inputs[3]); // y position of the next check point
    nextCheckpointDist = int.Parse(inputs[4]);
    nextCheckpointAngle = int.Parse(inputs[5]);
    inputs = Src(src2).Split(' ');
    int opponentX = int.Parse(inputs[0]);
    int opponentY = int.Parse(inputs[1]);

    _myPos = new Vector2(x, y);
    _myVelocity = new Vector2(-412, 227); //!_lastPos.HasValue ? (Vector2?) null : _myPos - _lastPos.Value;
    _speed = !_lastPos.HasValue ? 0 : (int) Vector2.Distance(_myPos, _lastPos.Value);

    _ePos = new Vector2(opponentX, opponentY);
    _eVelocity = !_eLastPos.HasValue ? Vector2.Zero : _ePos - _eLastPos.Value;

    _targetPos = new Vector2(nextCheckpointX, nextCheckpointY);
  }

  private static Vector2 Overstear(out double? angle)
  {
    angle = null;

    if (_speed < 50)
      return _targetPos;

    var targetVector = _targetPos - _myPos;

    angle = AngleBetween(_myVelocity.Value, targetVector);
    angle += (angle * 0.15);// * Math.Sign(angle.Value);

    var newT = Rotate(_myVelocity.Value, angle.Value);
    newT = Vector2.Normalize(newT) * Vector2.Distance(Vector2.Zero, targetVector);
    return newT + _myPos;
  }

  private static int CalcBreakTurningSpeed(Vector2 myPos, Vector2 targetPos, out int angle)
  {
    var velocityVector = myPos - _lastPos.Value;
    var targetVector = targetPos - myPos;


    angle = (int) (AngleBetween(velocityVector, targetVector));
    if (angle > 10)
      return (int) (130 * (angle / 180d));
    return 0;
  }

  private static int CalcBreakTurning(int nextCheckpointAngle, out int angle)
  {
    var breakTurning = 0;
    angle = Math.Abs(nextCheckpointAngle);
    if (angle > 90)
      breakTurning = (int)(130 * (angle / 180d));
    return breakTurning;
  }

  private static int CalcBreakClosing(int nextCheckpointDist)
  {
    const double brakingDist = 600 * 6;
    const double brakingForce = 50;
    if (nextCheckpointDist < brakingDist)
      return (int) (brakingForce * (1 - nextCheckpointDist / brakingDist));
    return 0;
  }

  static double AngleBetween(Vector2 vector1, Vector2 vector2)
  {
    double sin = vector1.X * vector2.Y - vector2.X * vector1.Y;
    double cos = vector1.X * vector2.X + vector1.Y * vector2.Y;

    return Math.Atan2(sin, cos) * (180 / Math.PI);
  }

  private const double DegToRad = Math.PI / 180;

  public static Vector2 Rotate(Vector2 v, double degrees)
  {
    return RotateRadians(v, degrees * DegToRad);
  }

  public static Vector2 RotateRadians(Vector2 v, double radians)
  {
    var ca = Math.Cos(radians);
    var sa = Math.Sin(radians);
    return new Vector2((float)(ca * v.X - sa * v.Y), (float)(sa * v.X + ca * v.Y));
  }
}