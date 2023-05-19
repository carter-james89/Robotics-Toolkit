using RoboticToolKit.Robotics.Servos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRoboticLimbSegment 
{
    public IServo[] GetServos();

    public float GetLength();

    public Vector3 GetEndPoint();

}
