//using RoboticsToolkit.Robotics;
using RoboticsToolkit.Robotics.Limbs;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics.Gaits
{
    public interface IGaitEventListener
    {
        public enum EventType
        {
            OnGaitCycleBegin,
            OnGaitCycleComplete,
            OnGaitReturnedHome
        }
        public struct GaitEventData
        {
            public EventType EventType;
           
            public IGait Gait;
            public GaitEventData(EventType eventType,  IGait gait)
            {
                EventType = eventType;
          
                Gait = gait;
            }
        }
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
        public void RunGait();
        public void SetNextCycle(Vector3 direction, ILimbPositioner[] limbPositioners, bool rotate);
        public bool IsRunning();
        public void SubscribeToEvents(IGaitEventListener listener);
        public void UnubscribeFromEvents(IGaitEventListener listener);
    }
}