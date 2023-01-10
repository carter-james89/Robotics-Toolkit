using RoboticsToolkit.Robotics;
using RoboticToolkit.Robotics.Gaits;
using RoboticToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class QuadrupedMLGaitController : Agent, IGaitController, IRobotEventListener
{
    [SerializeField]
    private Transform m_target;

    private IRobot m_robot;
    private bool m_running = false;

    private IRoboticLimb[] m_limbs;
    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode Begin");
        m_robot.ResetController();
        Destroy(gameObject.GetComponent<DecisionRequester>());
        m_running = false;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
        var dir = m_target.transform.localPosition - transform.localPosition;
        sensor.AddObservation(dir);
        sensor.AddObservation(dir.magnitude);
        sensor.AddObservation(Vector3.up - transform.up);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
      //  Debug.Log("on action recieved");
        base.OnActionReceived(actions);
        var flLimb = m_limbs[0].GetPositioner();
        //var frLimb = m_limbs[1].GetGameObject().GetComponentInChildren<LimbPositioner>();
        //var brLimb = m_limbs[2].GetGameObject().GetComponentInChildren<LimbPositioner>();
        //var blLimb = m_limbs[3].GetGameObject().GetComponentInChildren<LimbPositioner>();

        var actionVectors = new List<Vector3>();
        actionVectors.Add(new Vector3(actions.ContinuousActions[0], actions.ContinuousActions[1], actions.ContinuousActions[2]));
        actionVectors.Add(new Vector3(actions.ContinuousActions[3], actions.ContinuousActions[4], actions.ContinuousActions[5]));
        actionVectors.Add(new Vector3(actions.ContinuousActions[6], actions.ContinuousActions[7], actions.ContinuousActions[8]));
        actionVectors.Add(new Vector3(actions.ContinuousActions[9], actions.ContinuousActions[10], actions.ContinuousActions[11]));

        Vector3 range = new Vector3(.03f, .1f, .03f);
        for (int i = 0; i < 4; i++)
        {
            var localPos = actionVectors[i];
            if ((Mathf.Abs(localPos.x) > range.x) ||
                (Mathf.Abs(localPos.y) > range.y) ||
                (Mathf.Abs(localPos.z) > range.z))
            {
                AddReward(-1);
                EndEpisode();
                return;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            m_limbs[i].GetPositioner().SetLimbPosition(m_limbs[i].GetPositioner().GetGameObject().transform.TransformPoint(actionVectors[i]), false);
        }
        foreach (var limb in m_limbs)
        {
            limb.RunLimb(true);
        }
        CalculateRewards();
    }
    private void CalculateRewards()
    {
        if (Vector3.Angle(Vector3.up, transform.up) > 40)
        {
            Debug.Log("end episode angle");
            AddReward(-1);
            EndEpisode();
            return;
        }
        //var velocityReward = (m_target.transform.forward - m_robot.GetRobotData().Velocity).magnitude;
        var dir = m_target.transform.localPosition - transform.localPosition;
        AddReward(-dir.magnitude);

        Debug.Log(StepCount);
    }
    public IGaitController.Direction GetDirection()
    {
        throw new System.NotImplementedException();
    }

    public IGaitController.GaitPattern GetGaitPattern()
    {
        throw new System.NotImplementedException();
    }

    public void Initialize(IRobot robot)
    {
        m_robot = robot;
        m_limbs = robot.GetLimbs();
        robot.SubscribeToEvents(this);
    }

    public bool IsRunning()
    {
        return m_running;
       // return m_robot.
    }

    public void Run()
    {
      // Debug.Log("run ml gait");
        RequestDecision();
      //  RequestD
    }

    public void SetDirection(IGaitController.Direction direction)
    {
        throw new System.NotImplementedException();
    }

    public void SetGaitPattern(IGaitController.GaitPattern type)
    {
        throw new System.NotImplementedException();
    }

    public void OnRobotEventOccured(IRobotEventListener.EventData eventData)
    {
       // Debug.Log("got robot event");
        switch (eventData.EventType)
        {
            case IRobotEventListener.EventType.OnRobotInitialized:
                break;
            case IRobotEventListener.EventType.OnRobotInPosition:
                m_running = true;
                gameObject.AddComponent<DecisionRequester>();
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
