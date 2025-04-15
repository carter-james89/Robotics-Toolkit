using System.Collections;
using System.Collections.Generic;
using Toolkit.Utilities.Events;
using UnityEngine;

namespace RoboticsToolkit.Robotics.RoboticControllers
{
    public struct LimbValues
    {
        public Vector3 LimbTarget;
        public float[] ServoAngles;

        public LimbValues(Vector3 limbTarget, float[] servoAngles)
        {
            LimbTarget = limbTarget;
            ServoAngles = servoAngles;
        }
    }
    public interface IRoboticController : IEventSource<QuadrupedRoboticControllerEventData>
    {
        public bool Initialize(IRobot robot);
        public bool SetTransformValues();

        public void ResetController();

        public bool IsSimulator();

        public void SetRobotHeight(float height, float speed);

        public LimbValues[] CalculateLimbData(IRobot quadToControl);
    }
    public enum QuadrupedRoboticControllerEventType
    {
        OnControllerInitialized,
        OnHeightAdjustmentBegin,
        OnHeightAdjustmentEnd,
    }
    public class QuadrupedRoboticControllerEventData : IEventData
    {


        public QuadrupedRoboticControllerEventType EventType;
        public IRoboticController Controller;
        public IRobot Robot;
        public QuadrupedRoboticControllerEventData(QuadrupedRoboticControllerEventType eventType, IRoboticController controller, IRobot robot)
        {
            this.EventType = eventType;
            this.Controller = controller;
            this.Robot = robot;
        }
    }
}
