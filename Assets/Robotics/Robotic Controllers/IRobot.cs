using RoboticsToolkit.Gimbal;
using RoboticsToolkit.Robotics.Limbs;
using RoboticsToolkit.Robotics.RoboticControllers;
using UnityEngine;

namespace RoboticsToolkit.Robotics
{

    public interface IRobot 
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

        public GameObject GetGameObject();
        public IRoboticLimb[] GetLimbs();
        public RobotData GetRobotData();

        public IGimbal GetGimbal();

        public void EmergencyStop();

        public void ResetController();

        public void SubscribeToEvents(IRobotEventListener listener);
        public void UnsubscribeToEvents(IRobotEventListener listener);
    } 

    public interface IRobotEventListener
    {
        public enum EventType
        {
            OnRobotInitialized,
            OnRobotInPosition,
            OnRobotReady,
            OnLimbsPositioned,
            OnEmergencyStop,
            OnReset
        }
        public struct EventData
        {
            public EventType EventType;
            public IRobot Robot;
            public IRoboticController Controller;

            public EventData(EventType eventType, IRobot robot, IRoboticController controller)
            {
                EventType = eventType;
                Robot = robot;
                Controller = controller;
            }          
        }
        public void OnRobotEventOccured(EventData eventData);
    }
}
