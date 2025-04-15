using RoboticsToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using Toolkit.Utilities.Events;
using UnityEngine;

public interface ILimbPositioner : IEventSource<LimbPositionerEventData>
{
    public void RotateToPosition(Vector3 globalPosition, Quaternion rotationAxis, float time, bool localSpace);

    public void RotateToPosition(Vector3 direction, Vector3 upDirection, float distance, float time);

    public void TranslateToPosition(Vector3 globalPosition, float time, bool localSpace);

    public void TranslateToPosition(Vector3 direction, Vector3 upDir, float distance, float time);

    public bool LimbAtTarget();

    public void SetLimbPosition(Vector3 globalPosition, bool localSpace);

    public Vector3 GetLimbPosition(bool localSpace);

    public bool Run();
}



public class LimbPositionerEventData : IEventData
{
    public enum LimbPositionerEventType
    {
        OnAtTarget,
        OnMovementCanceled
    }

    public LimbPositionerEventType EventType;
    public IRoboticLimb Limb;
    public Vector3 TargetPosition;

    public LimbPositionerEventData(LimbPositionerEventType eventType, IRoboticLimb limb, Vector3 targetPosition)
    {
        EventType = eventType;
        Limb = limb;
        TargetPosition = targetPosition;
    }
}

