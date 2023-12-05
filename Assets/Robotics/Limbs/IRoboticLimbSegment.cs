using RoboticsToolkit.Robotics.Servos;
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

    public void SetServoAngle(int servo, float angle);
    public void SetServoAngle(float angle);
    public float GetServoAngle(int servo);
    public float GetServoAngle();

    public IServo GetServo(int servo);


    public float GetLength();

    public Vector3 GetEndPoint();

    public GameObject GetGameObject();

}
