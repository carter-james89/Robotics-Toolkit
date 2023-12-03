using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics
{
    public interface IServoCMDRelay
    {
        public GameObject GetGameObject();
        public bool Initialize(IRobot robot);

        public void ResetController();

        public bool IsSimulator();

        public bool RelayServoCommands(QuadrupedGroundStationData groundStationData);
    } 
}
