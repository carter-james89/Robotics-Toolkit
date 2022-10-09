using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics
{
    public class YertleControllerSimulation : MonoBehaviour, IRoboticController
    {
        public GameObject GetGameObject() => gameObject;

        public bool IsSimulator() => true;

        public bool SetTransformValues()
        {
            // throw new System.NotImplementedException();
            return true;
        }

        public bool Initialize(IRobot robot)
        {
            return true;
        }

        public bool SendCommands(QuadrupedGroundStationData groundStationData)
        {
            return true;
        }
    }

}