//using RoboticsToolkit.Robotics;
using RoboticsToolkit.Gimbal;
using RoboticsToolkit.Robotics.Limbs;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics.Gaits
{
    public enum EventType
    {
        OnGaitCycleBegin,
        OnGaitPointHit,
        OnGaitCycleComplete,
        OnGaitReturnedHome
    }
    public struct GaitEventData
    {
        public EventType EventType;

        public IGait Gait;
        public GaitEventData(EventType eventType, IGait gait)
        {
            EventType = eventType;

            Gait = gait;
        }
    }
    public interface IGaitEventListener
    {
        public void OnGaitEventOccured(GaitEventData eventData);
    }
    public interface IGait
    {
        public enum Direction
        {
            NONE,
            Forward,
            Backward,
            RotatingClockwise,
            RotatingCounterClockwise,
            StrafeLeft,
            StrafeRight,
        }

        public void Translate(ILimbPositioner[] limbPositioners, float speed, float strideLength, float strideHeight);
        public void Reset();
        public void Stop();
        public bool CheckLimbPositions(ILimbPositioner[] limbPositioners);
        public GaitCycleInfo GetGaitCycleInfo();

        public float GetRotationSpeedMultiplier();

        public bool IsRunning();
        public void SubscribeToEvents(IGaitEventListener listener);
        public void UnubscribeFromEvents(IGaitEventListener listener);
    }
}