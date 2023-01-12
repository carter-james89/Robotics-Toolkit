using RoboticsToolkit.Robotics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadrupedSimulationPositioner : MonoBehaviour, IQuadrupedPositioner, IRobotEventListener
{
    private IRobot m_robot;
    private Vector3 m_startPos = Vector3.zero;

    private bool m_firstSet = true;
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public bool Initialize(IRobot robot)
    {
        m_robot = robot;
        robot.SubscribeToEvents(this);
        m_startPos = m_robot.GetGameObject().transform.position;
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

    public void OnRobotEventOccured(IRobotEventListener.EventData eventData)
    {
        switch (eventData.EventType)
        {
            case IRobotEventListener.EventType.OnRobotInitialized:
                break;
            case IRobotEventListener.EventType.OnRobotInPosition:
                if (m_firstSet)
                {
                    m_startPos = m_robot.GetGameObject().transform.position;
                    m_firstSet = false;
                }
                break;
            case IRobotEventListener.EventType.OnEmergencyStop:
                break;
            case IRobotEventListener.EventType.OnReset:
                break;
            default:
                break;
        }
    }
}
