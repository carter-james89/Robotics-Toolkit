using RoboticsToolkit.Gimbal;
using RoboticsToolkit.Robotics.Limbs;
using RoboticsToolkit.Robotics.RoboticControllers;
using Toolkit.Utilities.Events;
using UnityEngine;

namespace RoboticsToolkit.Robotics
{

    public interface IRobot : IEventSource<RobotEventData>
    {
        public enum Status
        {
            NotReady,
            Initialized,
            AdjustingHeight,
            Ready,
        }

        public Status GetStatus();

        public struct RobotData
        {
            public Vector3 Velocity;
            public Vector3 AngularVelocity;

            public RobotData(Vector3 velocity, Vector3 angularVelocity)
            {
                Velocity = velocity;
                AngularVelocity = angularVelocity;
            }
        }

        public void Bootup();

        public bool IsSimulation();

        public void Run();

        public void SetLimbs(LimbValues[] limbData);

        public IRoboticLimb[] GetLimbs();
        public RobotData GetRobotData();

        public IGimbal GetGimbal();

        public void EmergencyStop();

        public void ResetController();

    }
    public enum RobotEventType
    {
        OnRobotInitialized,
        OnRobotInPosition,
        OnRobotReady,
        OnLimbsPositioned,
        OnEmergencyStop,
        OnReset
    }
    public class RobotEventData : IEventData
    {


        public RobotEventType EventType;
        public IRobot Robot;
        public IRoboticController Controller;

        public RobotEventData(RobotEventType eventType, IRobot robot, IRoboticController controller)
        {
            EventType = eventType;
            Robot = robot;
            Controller = controller;
        }

    }
}
