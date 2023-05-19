using RoboticToolKit.Robotics.Servos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoboticLimbSegment : MonoBehaviour, IRoboticLimbSegment
{
    public Vector3 GetEndPoint()
    {
       return transform.GetChild(0).localPosition;
    }

    public float GetLength()
    {
        return GetEndPoint().magnitude;
    }

    public IServo[] GetServos()
    {
        return GetComponents<IServo>(); 
    }
}
