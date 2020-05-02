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
class PlayerOld
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


  static void MainOld(string[] args)
  {
    int nextCheckpointDist = 0;
    int nextCheckpointAngle = 0;

    var breakClosing = Vector2.Distance(_myPos, _targetPos) < Vector2.Distance(_targetPos, _ePos)
      ? CalcBreakClosing(nextCheckpointDist)
      : 0;


    var thrust = 100d;
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


  private static Vector2 Overstear(out double? angle)
  {
    angle = null;

    if (_speed < 50)
      return _targetPos;

    var targetVector = _targetPos - _myPos;

    angle = _myVelocity.Value.Angle(targetVector);
    angle += (angle * 0.15); // * Math.Sign(angle.Value);

    var newT = _myVelocity.Value.Normalize().Rotate(angle.Value) * targetVector.Magnitude();
    return newT + _myPos;
  }

  private static int CalcBreakTurningSpeed(Vector2 myPos, Vector2 targetPos, out int angle)
  {
    var velocityVector = myPos - _lastPos.Value;
    var targetVector = targetPos - myPos;


    angle = (int) (velocityVector.Angle(targetVector));
    if (angle > 10)
      return (int) (130 * (angle / 180d));
    return 0;
  }

  private static int CalcBreakTurning(int nextCheckpointAngle, out int angle)
  {
    var breakTurning = 0;
    angle = Math.Abs(nextCheckpointAngle);
    if (angle > 90)
      breakTurning = (int) (130 * (angle / 180d));
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
}