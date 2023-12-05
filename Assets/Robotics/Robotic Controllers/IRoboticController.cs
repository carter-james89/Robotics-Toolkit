using System.Collections;
using System.Collections.Generic;
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
    public interface IRoboticController
    {
        public GameObject GetGameObject();
        public bool Initialize(IRobot robot);
        public bool SetTransformValues();

        public void ResetController();

        public bool IsSimulator();

        public void SetRobotHeight(float height, float speed);

        public LimbValues[] CalculateLimbData(IRobot quadToControl);
        public void SubscribeToControllerEvents(IRoboticControllerEventListener listener);
        public void UnsubscribeFromControllerEvents(IRoboticControllerEventListener listener);
    }

    public interface IRoboticControllerEventListener
    {
        public enum EventType
        {
            OnControllerInitialized,
            OnHeightAdjustmentBegin,
            OnHeightAdjustmentEnd,
        }
        public class QuadrupedRoboticControllerEvendData
        {
            public EventType EventType;
            public IRoboticController Controller;
            public IRobot Robot;
            public QuadrupedRoboticControllerEvendData(EventType eventType, IRoboticController controller, IRobot robot)
            {
                this.EventType = eventType;
                this.Controller = controller;
                this.Robot = robot;
            }
        }
        public void OnControllerEventOccured(QuadrupedRoboticControllerEvendData eventData);
    }
}
