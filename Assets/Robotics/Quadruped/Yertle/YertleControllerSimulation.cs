using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics
{
    public class YertleControllerSimulation : MonoBehaviour, IRoboticController
    {
        public GameObject GetGameObject() => gameObject;

        private Vector3 m_defaultPosition;
        private Quaternion m_defaultRotation;

        public bool IsSimulator() => true;

        private IRobot m_robot;

        public bool SetTransformValues()
        {
            // throw new System.NotImplementedException();
            return true;
        }

        public bool Initialize(IRobot robot)
        {
            m_robot = robot;
            m_defaultPosition = transform.localPosition;
            m_defaultRotation = transform.localRotation;
            return true;
        }

        public bool SendCommands(QuadrupedGroundStationData groundStationData)
        {
            return true;
        }

      
        public void ResetController()
        {
        //    Debug.Log("Teleport root to " + m_defaultPosition);
            var ab = m_robot.GetGameObject().GetComponent<ArticulationBody>();
            ab.TeleportRoot(m_defaultPosition, m_defaultRotation);
            ab.velocity = Vector3.zero;
            ab.angularVelocity = Vector3.zero;
        }

    }

}