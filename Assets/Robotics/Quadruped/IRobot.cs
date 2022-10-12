using RoboticToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics
{
    public interface IRobot 
    {
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
        public GameObject GetGameObject();
        public IRoboticLimb[] GetLimbs();
        public RobotData GetRobotData();

        public void EmergencyStop();

        public void ResetController();
    } 
}
