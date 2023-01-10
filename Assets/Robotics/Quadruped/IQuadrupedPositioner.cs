using RoboticsToolkit.Robotics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics
{
    public interface IQuadrupedPositioner
    {
        public GameObject GetGameObject();
        public bool Initialize(IRobot robot);
        public bool PositionTransform();

        public void BeginResetPositioner();
        public void CompletePositionerReset();

        public bool IsSimulator();
    }
}