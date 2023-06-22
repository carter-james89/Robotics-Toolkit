using RoboticToolKit.Robotics.Servos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRoboticLimbSegment 
{
    public enum RenderType
    {
        Line,
        Mesh
    }

    public void SetRenderType(RenderType type, Color color);
    public IServo[] GetServos();


    public float GetLength();

    public Vector3 GetEndPoint();

    public GameObject GetGameObject();

}
