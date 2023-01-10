using RoboticsToolkit.Robotics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadrupedSimulationPositioner : MonoBehaviour, IQuadrupedPositioner
{
    private IRobot m_robot;
    private Vector3 m_startPos;
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public bool Initialize(IRobot robot)
    {
        m_robot = robot;
        m_startPos = robot.GetGameObject().transform.localPosition;
        return true;
    }

    public bool IsSimulator()
    {
        return true;
    }

    public bool PositionTransform()
    {
        return true;
    }
    private bool m_resetting = false;
    public void BeginResetPositioner()
    {
        m_resetting = true;
       
    }
    public void CompletePositionerReset()
    {
        m_resetting = false;
    }
    private void FixedUpdate()
    {
        if (m_resetting)
        {
            var ab = m_robot.GetGameObject().GetComponent<ArticulationBody>();

            // ab.TeleportRoot(m_ground.position + new Vector3(0, m_startHeight, 0), Quaternion.identity);
            ab.TeleportRoot(m_startPos, Quaternion.identity);
            ab.velocity = Vector3.zero;
            ab.angularVelocity = Vector3.zero;
        }
    }
}
