//using RoboticsToolkit.Robotics;
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
        public void SetStrideDistance(float distance);
        public void SetStrideHeight(float height);
        public void SetStrideValues(float strideDistance, float strideHeight);
        public void Initialize();
        public void Begin();
        public bool RequestBeginCMD(IGaitController requestingController, ILimbPositioner[] limbPositioners);
        public void ReturnHome();
        public void Stop();
        public void CheckLimbPositions(ILimbPositioner[] limbPositioners);
        public void SetNextCycle(Vector3 direction, ILimbPositioner[] limbPositioners, float speed, bool rotate);
        public bool IsRunning();
        public void SubscribeToEvents(IGaitEventListener listener);
        public void UnubscribeFromEvents(IGaitEventListener listener);
    }
}