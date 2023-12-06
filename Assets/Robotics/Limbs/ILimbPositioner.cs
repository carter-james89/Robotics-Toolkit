using RoboticsToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILimbPositioner
{
    public GameObject GetGameObject();
    public void RotateToPosition(Vector3 globalPosition, Quaternion rotationAxis, float time, bool localSpace);

    public void RotateToPosition(Vector3 direction, Vector3 upDirection, float distance, float time);

    public void TranslateToPosition(Vector3 globalPosition, float time, bool localSpace);

    public void TranslateToPosition(Vector3 direction, Vector3 upDir, float distance, float time);

    public bool LimbAtTarget();

    public void SetLimbPosition(Vector3 globalPosition, bool localSpace);

    public Vector3 GetLimbPosition(bool localSpace);

    public bool Run();

    public void SubscribeToEvents(ILimbPositionerEventListener listener);
    public void UnsubscribeFromEvents(ILimbPositionerEventListener listener);
}



public interface ILimbPositionerEventListener
{
    public enum EventType
    {
        OnAtTarget,
        OnMovementCanceled
    }
    public struct EventData
    {   
        public EventType EventType;
        public IRoboticLimb Limb;
        public Vector3 TargetPosition;

        public EventData(EventType eventType, IRoboticLimb limb, Vector3 targetPosition)
        {
            EventType = eventType;
            Limb = limb;
            TargetPosition = targetPosition;
        }
    }

    public void OnLimbPositionerEventOccured(EventData eventData);
}
