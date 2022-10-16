using RoboticToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILimbPositioner 
{
    public void RotateToPosition(Vector3 globalPosition, Quaternion rotationAxis, float time);

    public void TranslateToPosition(Vector3 globalPosition, float time);

    public void SetLimbPosition(Vector3 globalPosition);

    public Vector3 GetLimbPosition();

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
