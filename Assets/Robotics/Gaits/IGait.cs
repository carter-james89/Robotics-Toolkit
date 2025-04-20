//using RoboticsToolkit.Robotics;
using RoboticsToolkit.Gimbal;
using RoboticsToolkit.Robotics.Limbs;
using System.Collections.Generic;
using Toolkit.Utilities.Events;
using UnityEngine;

namespace RoboticsToolkit.Robotics.Gaits
{
    public struct GaitCycleInfo
    {
        public int[] RotatingLimbs;
        public int[] TranslatingLimbs;
    }
    public enum GaitEventType
    {
        OnGaitCycleBegin,
        OnGaitPointHit,
        OnGaitCycleComplete,
        OnGaitReturnedHome
    }
    public struct GaitEventData : IEventData
    {
        public GaitEventType EventType;

        public IGait Gait;
        public GaitEventData(GaitEventType eventType, IGait gait)
        {
            EventType = eventType;

            Gait = gait;
        }
    }

    public interface IGait : IEventSource<GaitEventData>
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
 
    }
}