using RoboticsToolkit.Robotics;
using RoboticsToolkit.Robotics.Gaits;
using RoboticsToolkit.Robotics.Limbs;
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

    private ArticulationBody m_rigidBody;

    private IRoboticLimb[] m_limbs;
    public override void OnEpisodeBegin()
    {
        //Debug.Log("Episode Begin");
        m_robot.ResetController();
        // Destroy(gameObject.GetComponent<DecisionRequester>());
        m_running = false;
        m_calculateReward = false;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
       // var dir = m_target.transform.localPosition - transform.localPosition;
        // sensor.AddObservation(m_positionRange);
        sensor.AddObservation(m_target.transform.forward);
        // sensor.AddObservation(dir.magnitude);
        sensor.AddObservation(Vector3.up - transform.up);

        sensor.AddObservation(m_rigidBody.velocity);

        foreach (var limb in m_limbs)
        {
            sensor.AddObservation(m_robot.GetGameObject().transform.InverseTransformPoint(limb.GetEndPoint().transform.position));
        }
    }

    private Vector3 m_positionRange = new Vector3(.03f, .07f, .03f);
    public override void OnActionReceived(ActionBuffers actions)
    {
        //Debug.Log("on action recieved");
        base.OnActionReceived(actions);
        //var flLimb = m_limbs[0].GetPositioner();
        //var frLimb = m_limbs[1].GetGameObject().GetComponentInChildren<LimbPositioner>();
        //var brLimb = m_limbs[2].GetGameObject().GetComponentInChildren<LimbPositioner>();
        //var blLimb = m_limbs[3].GetGameObject().GetComponentInChildren<LimbPositioner>();

        //var actionVectors = new List<Vector3>();
        //actionVectors.Add(new Vector3(actions.ContinuousActions[0], actions.ContinuousActions[1], actions.ContinuousActions[2]));
        //actionVectors.Add(new Vector3(actions.ContinuousActions[3], actions.ContinuousActions[4], actions.ContinuousActions[5]));
        //actionVectors.Add(new Vector3(actions.ContinuousActions[6], actions.ContinuousActions[7], actions.ContinuousActions[8]));
        //actionVectors.Add(new Vector3(actions.ContinuousActions[9], actions.ContinuousActions[10], actions.ContinuousActions[11]));

        var actionVectors = new List<Vector3>();
        actionVectors.Add(new Vector3(actions.DiscreteActions[0], actions.DiscreteActions[1], actions.DiscreteActions[2]));
        actionVectors.Add(new Vector3(actions.DiscreteActions[3], actions.DiscreteActions[4], actions.DiscreteActions[5]));
        actionVectors.Add(new Vector3(actions.DiscreteActions[6], actions.DiscreteActions[7], actions.DiscreteActions[8]));
        actionVectors.Add(new Vector3(actions.DiscreteActions[9], actions.DiscreteActions[10], actions.DiscreteActions[11]));

        for (int i = 0; i < actionVectors.Count; i++)
        {
            var tempPos = actionVectors[i];
            var xPercent = actionVectors[i].x / m_positionRange.x;
            tempPos.x = 1 * Mathf.Lerp(-m_positionRange.x, m_positionRange.x, xPercent);

            var yPercent = actionVectors[i].y / m_positionRange.y;
            tempPos.y = 1 * Mathf.Lerp(-m_positionRange.y, m_positionRange.y, yPercent);

            var zPercent = actionVectors[i].z / m_positionRange.z;
            tempPos.z = 1 * Mathf.Lerp(-m_positionRange.z, m_positionRange.z, zPercent);

            actionVectors[i] = tempPos;
        }

        for (int i = 0; i < 4; i++)
        {
            m_limbs[i].GetPositioner().SetLimbPosition(m_limbs[i].GetPositioner().GetGameObject().transform.TransformPoint(actionVectors[i]), false);
        }

        m_calculateReward = true;
        //foreach (var limb in m_limbs)
        //{
        //    limb.RunLimb(true);
        //}
        // CalculateRewards();
    }
    private void CalculateRewards()
    {
        if (Vector3.Angle(Vector3.up, transform.up) > 20)
        {
            //Debug.Log("end episode angle");
            AddReward(-1);
            EndEpisode();

            return;
        }

        var dir = m_target.transform.localPosition - transform.localPosition;

        //AddReward();

        // if(Mathf.Abs(dir.y) > .1f)
        //if(transform.localPosition.y  > .25f)
        // {
        //     //Debug.Log("end episode angle");
        //     AddReward(-1);
        //     EndEpisode();

        //     return;
        // }
        //var velocityReward = (m_target.transform.forward - m_robot.GetRobotData().Velocity).magnitude;
        // var dir = m_target.transform.localPosition - transform.localPosition;
        // AddReward(-dir.magnitude);
        //  Vector3.
        AddReward(Vector3.Dot(m_rigidBody.velocity, m_target.forward));
      //  Debug.Log(Vector3.Dot(m_rigidBody.velocity, m_target.forward));
       // AddReward(.01f);

        m_calculateReward = false;
        // Debug.Log(StepCount);
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
        m_rigidBody = m_robot.GetGameObject().GetComponent<ArticulationBody>();    
    }

    public bool IsRunning()
    {
        return m_running;
        // return m_robot.
    }


    private bool m_calculateReward = false;
    public void Run()
    {
        if (m_calculateReward)
        {
           // CalculateRewards();
        }      
       // Debug.Log("requestDecision");
        RequestDecision();
        //  RequestAction();

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
                //gameObject.AddComponent<DecisionRequester>();
                break;
            case IRobotEventListener.EventType.OnEmergencyStop:
                break;
            case IRobotEventListener.EventType.OnReset:

                break;
            case IRobotEventListener.EventType.OnLimbsPositioned:
                CalculateRewards();
                break;
            default:
                break;
        }
    }

    public void Run(IRoboticLimb[] mirrorLimbs, ILimbPositioner[] limbs)
    {
        throw new System.NotImplementedException();
    }

    public void BeginMovement(ILimbPositioner[] limbs, IGaitController.GaitPattern patern, Vector3 direction, bool rotate)
    {
        throw new System.NotImplementedException();
    }
}
